using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Image;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SlideshowFrame : UdonSharpBehaviour
{
    [SerializeField, Tooltip("URLs of images to load")]
    private VRCUrl[] imageUrls;
    
    [SerializeField, Tooltip("URL of text file containing captions for images, one caption per line.")]
    private VRCUrl stringUrl;
    
    [SerializeField, Tooltip("Renderer to show downloaded images on.")]
    private new Renderer renderer;
    
    [SerializeField, Tooltip("Text field for captions.")]
    private TMP_Text field;
    
    [SerializeField, Tooltip("Duration in seconds until the next image is shown.")]
    private float slideDurationSeconds = 10f;
    
    [SerializeField, Tooltip("Retry delay when an image fails to load.")]
    private float retryDelaySeconds = 2f;
    
    [SerializeField, Tooltip("Maximum retry attempts per image before skipping.")]
    private int maxRetryAttempts = 3;
    
    private int _loadedIndex = -1;
    private int _currentRetryCount = 0;
    private int _lastFailedIndex = -1;
    private bool _isInitialized = false;
    private bool _isLoading = false;
    private VRCImageDownloader _imageDownloader;
    private IUdonEventReceiver _udonEventReceiver;
    private string[] _captions = new string[0];
    private Texture2D[] _downloadedTextures;
    private Material _targetMaterial;
    private TextureInfo _textureInfo;
    
    private const string LOG_PREFIX = "[SlideshowFrame] ";
    
    
    
    private void Start()
    {
        Debug.Log($"{LOG_PREFIX}Start() called on {gameObject.name}");
        
        if (renderer == null)
        {
            Debug.LogError($"{LOG_PREFIX}Renderer reference is missing on {gameObject.name}. Disabling.");
            enabled = false;
            return;
        }

        if (slideDurationSeconds <= 0f)
        {
            Debug.LogWarning($"{LOG_PREFIX}slideDurationSeconds must be > 0. Defaulting to 10 seconds.");
            slideDurationSeconds = 10f;
        }
        
        if (retryDelaySeconds <= 0f)
        {
            retryDelaySeconds = 2f;
        }

        if (imageUrls == null || imageUrls.Length == 0)
        {
            Debug.LogError($"{LOG_PREFIX}No image URLs assigned on {gameObject.name}. Disabling.");
            enabled = false;
            return;
        }
        
        // Validate all URLs upfront
        int validUrlCount = 0;
        for (int i = 0; i < imageUrls.Length; i++)
        {
            if (imageUrls[i] != null && !string.IsNullOrEmpty(imageUrls[i].Get()))
            {
                validUrlCount++;
            }
            else
            {
                Debug.LogWarning($"{LOG_PREFIX}Image URL at index {i} is null or empty.");
            }
        }
        
        if (validUrlCount == 0)
        {
            Debug.LogError($"{LOG_PREFIX}All image URLs are invalid on {gameObject.name}. Disabling.");
            enabled = false;
            return;
        }
        
        Debug.Log($"{LOG_PREFIX}Found {validUrlCount}/{imageUrls.Length} valid image URLs.");

        // Downloaded textures will be cached in a texture array.
        _downloadedTextures = new Texture2D[imageUrls.Length];

        // Use a consistent material instance (avoid mixing sharedMaterial and material).
        _targetMaterial = renderer.material;
        
        if (_targetMaterial == null)
        {
            Debug.LogError($"{LOG_PREFIX}Failed to get material from renderer on {gameObject.name}. Disabling.");
            enabled = false;
            return;
        }

        _textureInfo = new TextureInfo();
        _textureInfo.GenerateMipMaps = true;
        _textureInfo.WrapModeV = TextureWrapMode.Clamp;
        _textureInfo.WrapModeU = TextureWrapMode.Clamp;
        
        // It's important to store the VRCImageDownloader as a variable, to stop it from being garbage collected!
        _imageDownloader = new VRCImageDownloader();
        
        if (_imageDownloader == null)
        {
            Debug.LogError($"{LOG_PREFIX}Failed to create VRCImageDownloader on {gameObject.name}. Disabling.");
            enabled = false;
            return;
        }
        
        // To receive Image and String loading events, 'this' is casted to the type needed
        _udonEventReceiver = (IUdonEventReceiver)this;
        
        _isInitialized = true;
        Debug.Log($"{LOG_PREFIX}Initialization complete on {gameObject.name}.");
        
        // Captions are downloaded once. On success, OnStringLoadSuccess() will be called.
        // If it fails, we still start the slideshow without captions.
        if (stringUrl != null && !string.IsNullOrEmpty(stringUrl.Get()))
        {
            Debug.Log($"{LOG_PREFIX}Loading captions from URL...");
            VRCStringDownloader.LoadUrl(stringUrl, _udonEventReceiver);
        }
        else
        {
            Debug.LogWarning($"{LOG_PREFIX}Caption URL is empty. Starting slideshow without captions.");
            LoadNext();
        }
    }
    
    public void LoadNext()
    {
        // Prevent re-entrant calls while loading
        if (_isLoading)
        {
            Debug.LogWarning($"{LOG_PREFIX}LoadNext() called while already loading. Ignoring.");
            return;
        }
        
        // Safety check: ensure we're properly initialized
        if (!_isInitialized)
        {
            Debug.LogError($"{LOG_PREFIX}LoadNext() called but not initialized. Attempting re-init...");
            Start();
            return;
        }
        
        // Safety check: ensure arrays are initialized and have content
        if (renderer == null || imageUrls == null || imageUrls.Length == 0 || _downloadedTextures == null)
        {
            int imageUrlsLength = imageUrls != null ? imageUrls.Length : 0;
            int texturesLength = _downloadedTextures != null ? _downloadedTextures.Length : 0;
            Debug.LogError($"{LOG_PREFIX}Arrays not properly initialized. renderer={renderer != null}, imageUrls={imageUrlsLength}, textures={texturesLength}");
            return;
        }

        if (_targetMaterial == null)
        {
            Debug.LogWarning($"{LOG_PREFIX}Target material was null, attempting to recover...");
            _targetMaterial = renderer.material;
            if (_targetMaterial == null)
            {
                Debug.LogError($"{LOG_PREFIX}Failed to recover material. Aborting.");
                return;
            }
        }
        
        if (_imageDownloader == null)
        {
            Debug.LogWarning($"{LOG_PREFIX}ImageDownloader was null, recreating...");
            _imageDownloader = new VRCImageDownloader();
            if (_imageDownloader == null)
            {
                Debug.LogError($"{LOG_PREFIX}Failed to recreate ImageDownloader. Aborting.");
                return;
            }
        }

        // All clients share the same server time. That's used to sync the currently displayed image.
        double serverTimeSeconds = Networking.GetServerTimeInMilliseconds() / 1000.0;
        _loadedIndex = (int)(serverTimeSeconds / slideDurationSeconds) % imageUrls.Length;
        
        // Ensure index is valid (handle potential negative modulo issues)
        if (_loadedIndex < 0)
        {
            _loadedIndex = 0;
        }
        
        Debug.Log($"{LOG_PREFIX}Loading image index {_loadedIndex}/{imageUrls.Length - 1} (serverTime={serverTimeSeconds:F1}s)");
        
        // Validate the URL at this index
        VRCUrl currentUrl = imageUrls[_loadedIndex];
        if (currentUrl == null || string.IsNullOrEmpty(currentUrl.Get()))
        {
            Debug.LogWarning($"{LOG_PREFIX}URL at index {_loadedIndex} is invalid. Skipping to next.");
            ScheduleNextLoad(1f);
            return;
        }

        var nextTexture = _downloadedTextures[_loadedIndex];
        
        if (nextTexture != null)
        {
            // Image already downloaded! No need to download it again.
            Debug.Log($"{LOG_PREFIX}Using cached texture for index {_loadedIndex}");
            _targetMaterial.mainTexture = nextTexture;
            CorrectImageSize(nextTexture);
            
            UpdateCaptionText();
            ScheduleNextLoad(slideDurationSeconds);
        }
        else
        {
            _isLoading = true;
            Debug.Log($"{LOG_PREFIX}Downloading image from: {currentUrl.Get()}");
            _imageDownloader.DownloadImage(currentUrl, _targetMaterial, _udonEventReceiver, _textureInfo);
        }
    }
    
    private void ScheduleNextLoad(float delay)
    {
        if (delay <= 0f)
        {
            delay = 1f;
        }
        SendCustomEventDelayedSeconds(nameof(LoadNext), delay);
    }

    private void UpdateCaptionText()
    {
        if (field == null)
        {
            return;
        }

        if (_captions != null && _loadedIndex >= 0 && _loadedIndex < _captions.Length)
        {
            string caption = _captions[_loadedIndex];
            field.text = caption != null ? caption : "";
        }
        else
        {
            field.text = "";
        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        if (result == null || string.IsNullOrEmpty(result.Result))
        {
            Debug.LogWarning($"{LOG_PREFIX}String load succeeded but result is null or empty.");
            _captions = new string[0];
        }
        else
        {
            _captions = result.Result.Replace("\r", "").Split('\n');
            Debug.Log($"{LOG_PREFIX}Captions loaded successfully: {_captions.Length} entries.");
        }

        // Load the next image. Then do it again, and again, and...
        LoadNext();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        string errorMsg = result != null ? result.Error.ToString() : "Unknown error";
        Debug.LogError($"{LOG_PREFIX}Could not load captions: {errorMsg}");

        _captions = new string[0];
        
        // Still start slideshow (captions will remain empty).
        LoadNext();
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        _isLoading = false;
        _currentRetryCount = 0;
        
        if (result == null || result.Result == null)
        {
            Debug.LogError($"{LOG_PREFIX}Image load success callback but result is null!");
            ScheduleNextLoad(retryDelaySeconds);
            return;
        }
        
        Debug.Log($"{LOG_PREFIX}Image loaded successfully: {result.SizeInMemoryBytes} bytes, {result.Result.width}x{result.Result.height}");
        
        if (_loadedIndex >= 0 && _loadedIndex < _downloadedTextures.Length)
        {
            _downloadedTextures[_loadedIndex] = result.Result;
        }
        else
        {
            int texturesLength = _downloadedTextures != null ? _downloadedTextures.Length : 0;
            Debug.LogWarning($"{LOG_PREFIX}Loaded index {_loadedIndex} out of range for texture cache (length={texturesLength})");
        }
        
        CorrectImageSize(result.Result);
        
        UpdateCaptionText();
        ScheduleNextLoad(slideDurationSeconds);
    }
    
    void CorrectImageSize(Texture2D texture)
    {
        if (_targetMaterial == null)
        {
            Debug.LogWarning($"{LOG_PREFIX}CorrectImageSize: _targetMaterial is null.");
            return;
        }
        
        if (texture == null)
        {
            Debug.LogWarning($"{LOG_PREFIX}CorrectImageSize: texture is null.");
            return;
        }
        
        if (renderer == null)
        {
            Debug.LogWarning($"{LOG_PREFIX}CorrectImageSize: renderer is null.");
            return;
        }
        
        // Prevent division by zero
        if (texture.height <= 0 || texture.width <= 0)
        {
            Debug.LogWarning($"{LOG_PREFIX}CorrectImageSize: Invalid texture dimensions {texture.width}x{texture.height}.");
            return;
        }

        float aspectRatio = (float)texture.width / texture.height;
        
        Vector3 screenScale = renderer.transform.localScale;
        
        // Prevent division by zero
        if (Mathf.Approximately(screenScale.y, 0f) || Mathf.Approximately(screenScale.x, 0f))
        {
            Debug.LogWarning($"{LOG_PREFIX}CorrectImageSize: Invalid screen scale {screenScale}.");
            return;
        }
        
        float screenAspectRatio = screenScale.x / screenScale.y;
        
        Debug.Log($"{LOG_PREFIX}CorrectImageSize: texture={texture.width}x{texture.height}, aspectRatio={aspectRatio:F2}, screenAspectRatio={screenAspectRatio:F2}");

        if (aspectRatio > screenAspectRatio || (aspectRatio < 1 && aspectRatio < screenAspectRatio))
        {
            float effectiveScreenAspectRatio = screenAspectRatio;
            if (effectiveScreenAspectRatio < 1)
                effectiveScreenAspectRatio = 1;

            float scaleY = (1f / effectiveScreenAspectRatio) * screenScale.y * aspectRatio;
            float offsetY = (1f - scaleY) / 2f;
            
            _targetMaterial.mainTextureScale = new Vector2(1, scaleY);
            _targetMaterial.mainTextureOffset = new Vector2(0, offsetY);
        }
        else
        {
            // Prevent division by zero for aspect ratio
            if (Mathf.Approximately(aspectRatio, 0f))
            {
                Debug.LogWarning($"{LOG_PREFIX}CorrectImageSize: aspectRatio is zero, skipping correction.");
                return;
            }
            
            float scaleX = (1f / aspectRatio) * screenScale.x;
            float offsetX = (1f - scaleX) / 2f;
            
            _targetMaterial.mainTextureScale = new Vector2(scaleX, 1);
            _targetMaterial.mainTextureOffset = new Vector2(offsetX, 0);
        }
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        _isLoading = false;
        
        string errorType = result != null ? result.Error.ToString() : "Unknown";
        string errorMsg = result != null ? result.ErrorMessage : "No details";
        
        Debug.LogError($"{LOG_PREFIX}Image load failed for index {_loadedIndex}: {errorType} - {errorMsg}");
        
        // Track retries for the same image
        if (_lastFailedIndex == _loadedIndex)
        {
            _currentRetryCount++;
        }
        else
        {
            _lastFailedIndex = _loadedIndex;
            _currentRetryCount = 1;
        }
        
        if (_currentRetryCount >= maxRetryAttempts)
        {
            Debug.LogWarning($"{LOG_PREFIX}Max retries ({maxRetryAttempts}) reached for index {_loadedIndex}. Moving to next image.");
            _currentRetryCount = 0;
            _lastFailedIndex = -1;
        }

        // Don't stall the slideshow forever on a failed image.
        ScheduleNextLoad(retryDelaySeconds);
    }

    private void OnDestroy()
    {
        Debug.Log($"{LOG_PREFIX}OnDestroy called on {gameObject.name}");
        
        if (_imageDownloader != null)
        {
            _imageDownloader.Dispose();
            _imageDownloader = null;
        }
        
        _isInitialized = false;
    }
}