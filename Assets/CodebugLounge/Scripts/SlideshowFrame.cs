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
    
    private int _loadedIndex = -1;
    private VRCImageDownloader _imageDownloader;
    private IUdonEventReceiver _udonEventReceiver;
    private string[] _captions = new string[0];
    private Texture2D[] _downloadedTextures;
    private Material _targetMaterial;
    private TextureInfo _textureInfo;
    
    
    
    private void Start()
    {
        if (renderer == null)
        {
            Debug.LogError("SlideshowFrame: Renderer reference is missing.");
            return;
        }

        if (slideDurationSeconds <= 0f)
        {
            Debug.LogWarning("SlideshowFrame: slideDurationSeconds must be > 0. Defaulting to 10 seconds.");
            slideDurationSeconds = 10f;
        }

        if (imageUrls == null || imageUrls.Length == 0)
        {
            Debug.LogError("SlideshowFrame: No image URLs assigned.");
            return;
        }

        // Downloaded textures will be cached in a texture array.
        _downloadedTextures = new Texture2D[imageUrls.Length];

        // Use a consistent material instance (avoid mixing sharedMaterial and material).
        _targetMaterial = renderer.material;

        _textureInfo = new TextureInfo();
        _textureInfo.GenerateMipMaps = true;
        _textureInfo.WrapModeV = TextureWrapMode.Clamp;
        _textureInfo.WrapModeU = TextureWrapMode.Clamp;
        
        // It's important to store the VRCImageDownloader as a variable, to stop it from being garbage collected!
        _imageDownloader = new VRCImageDownloader();
        
        // To receive Image and String loading events, 'this' is casted to the type needed
        _udonEventReceiver = (IUdonEventReceiver)this;
        
        // Captions are downloaded once. On success, OnStringLoadSuccess() will be called.
        // If it fails, we still start the slideshow without captions.
        if (stringUrl != null && stringUrl.Get() != null && stringUrl.Get().Length > 0)
        {
            VRCStringDownloader.LoadUrl(stringUrl, _udonEventReceiver);
        }
        else
        {
            Debug.LogWarning("SlideshowFrame: Caption URL is empty. Starting slideshow without captions.");
            LoadNext();
        }
        
       
    }
    
    public void LoadNext()
    {
        // Safety check: ensure arrays are initialized and have content
        if (renderer == null || imageUrls == null || imageUrls.Length == 0 || _downloadedTextures == null)
        {
            Debug.LogError("SlideshowFrame: Arrays not properly initialized");
            return;
        }

        if (_targetMaterial == null)
        {
            _targetMaterial = renderer.material;
        }

        // All clients share the same server time. That's used to sync the currently displayed image.
        _loadedIndex = (int)(Networking.GetServerTimeInMilliseconds() / 1000f / slideDurationSeconds) % imageUrls.Length;
        
        Debug.Log($"Loading image index {_loadedIndex}");

        var nextTexture = _downloadedTextures[_loadedIndex];
        
        if (nextTexture != null)
        {
            // Image already downloaded! No need to download it again.
            _targetMaterial.mainTexture = nextTexture;
            CorrectImageSize(nextTexture);
            
            UpdateCaptionText();
            SendCustomEventDelayedSeconds(nameof(LoadNext), slideDurationSeconds);
        }
        else
        {
            _imageDownloader.DownloadImage(imageUrls[_loadedIndex], _targetMaterial, _udonEventReceiver, _textureInfo);
        }
    }

    private void UpdateCaptionText()
    {
        if (field == null)
        {
            return;
        }

        if (_loadedIndex < _captions.Length)
        {
            field.text = _captions[_loadedIndex];
        }
        else
        {
            field.text = "";
        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        _captions = result.Result.Replace("\r", "").Split('\n');

        Debug.Log($"Captions loaded: {_captions.Length} entries. Starting slideshow.");
        // Load the next image. Then do it again, and again, and...
        LoadNext();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError($"Could not load string {result.Error}");

        // Still warn + start slideshow (captions will remain empty).
        LoadNext();
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        //Debug.Log($"Image loaded: {result.SizeInMemoryBytes} bytes.");
        
        if (_loadedIndex >= 0 && _loadedIndex < _downloadedTextures.Length)
        {
            _downloadedTextures[_loadedIndex] = result.Result;
        }
        
        CorrectImageSize(result.Result);
        
        UpdateCaptionText();
        SendCustomEventDelayedSeconds(nameof(LoadNext), slideDurationSeconds);
    }
    
    void CorrectImageSize(Texture2D texture)
    {
        if (_targetMaterial == null)
        {
            return;
        }

        float aspectRatio = (float)texture.width / texture.height;
        
        Vector3 screenScale = renderer.transform.localScale;
        float screenAspectRatio = (float)screenScale.x / screenScale.y;
        //Debug.Log($"Correcting image size: {texture.width}x{texture.height}, aspect ratio: {aspectRatio}, screen aspect ratio: {screenAspectRatio}");

        if (aspectRatio > screenAspectRatio || (aspectRatio < 1 && aspectRatio < screenAspectRatio))
        {
            if (screenAspectRatio < 1)
                screenAspectRatio = 1;

            _targetMaterial.mainTextureScale = new Vector2(1, 1 / screenAspectRatio * screenScale.y * aspectRatio);
            _targetMaterial.mainTextureOffset = new Vector2(0, (1 - (1 / screenAspectRatio) *screenScale.y * aspectRatio) / 2);

        }
        else
        {
            //might need the screen aspect ratio adjustment and if < 1 but it's fine for now

            _targetMaterial.mainTextureScale = new Vector2((1 / aspectRatio) * screenScale.x, 1);
            _targetMaterial.mainTextureOffset = new Vector2((1 - (1 / aspectRatio) * screenScale.x) / 2, 0);
        }
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.Log($"Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");

        // Don't stall the slideshow forever on a failed image.
        SendCustomEventDelayedSeconds(nameof(LoadNext), 1f);
    }

    private void OnDestroy()
    {
        if (_imageDownloader != null)
        {
            _imageDownloader.Dispose();
        }
    }
}