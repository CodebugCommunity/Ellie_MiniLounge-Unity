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
    
    
    
    private void Start()
    {
        // Downloaded textures will be cached in a texture array.
        _downloadedTextures = new Texture2D[imageUrls.Length];
        
        // It's important to store the VRCImageDownloader as a variable, to stop it from being garbage collected!
        _imageDownloader = new VRCImageDownloader();
        
        // To receive Image and String loading events, 'this' is casted to the type needed
        _udonEventReceiver = (IUdonEventReceiver)this;
        
        // Captions are downloaded once. On success, OnImageLoadSuccess() will be called.
        VRCStringDownloader.LoadUrl(stringUrl, _udonEventReceiver);
        
       
    }
    
    public void LoadNext()
    {
        // Safety check: ensure arrays are initialized and have content
        if (imageUrls == null || imageUrls.Length == 0 || _downloadedTextures == null)
        {
            Debug.LogError("SlideshowFrame: Arrays not properly initialized");
            return;
        }
    
        // All clients share the same server time. That's used to sync the currently displayed image.
        _loadedIndex = (int)(Networking.GetServerTimeInMilliseconds() / 1000f / slideDurationSeconds) % imageUrls.Length;

        var nextTexture = _downloadedTextures[_loadedIndex];
        
        if (nextTexture != null)
        {
            // Image already downloaded! No need to download it again.
            renderer.sharedMaterial.mainTexture = nextTexture;
            CorrectImageSize(nextTexture);
            
            UpdateCaptionText();
            SendCustomEventDelayedSeconds(nameof(LoadNext), slideDurationSeconds);
        }
        else
        {
            var rgbInfo = new TextureInfo();
            rgbInfo.GenerateMipMaps = true;
            rgbInfo.WrapModeV = TextureWrapMode.Clamp;
            rgbInfo.WrapModeU = TextureWrapMode.Clamp;
            _imageDownloader.DownloadImage(imageUrls[_loadedIndex], renderer.material, _udonEventReceiver, rgbInfo);
        }
    }

    private void UpdateCaptionText()
    {
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
        _captions = result.Result.Split('\n');

        Debug.Log($"Captions loaded: {_captions.Length} entries. Starting slideshow.");
        // Load the next image. Then do it again, and again, and...
        LoadNext();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError($"Could not load string {result.Error}");
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        //Debug.Log($"Image loaded: {result.SizeInMemoryBytes} bytes.");
        
        _downloadedTextures[_loadedIndex] = result.Result;
        
        CorrectImageSize(result.Result);
        
        UpdateCaptionText();
        SendCustomEventDelayedSeconds(nameof(LoadNext), slideDurationSeconds);
    }
    
    void CorrectImageSize(Texture2D texture)
    {
        float aspectRatio = (float)texture.width / texture.height;
        
        Vector3 screenScale = renderer.transform.localScale;
        float screenAspectRatio = (float)screenScale.x / screenScale.y;
        //Debug.Log($"Correcting image size: {texture.width}x{texture.height}, aspect ratio: {aspectRatio}, screen aspect ratio: {screenAspectRatio}");

        if (aspectRatio > screenAspectRatio || (aspectRatio < 1 && aspectRatio < screenAspectRatio))
        {
            if (screenAspectRatio < 1)
                screenAspectRatio = 1;

            renderer.sharedMaterial.mainTextureScale = new Vector2(1, 1 / screenAspectRatio * screenScale.y * aspectRatio);
            renderer.sharedMaterial.mainTextureOffset = new Vector2(0, (1 - (1 / screenAspectRatio) *screenScale.y * aspectRatio) / 2);

        }
        else
        {
            //might need the screen aspect ratio adjustment and if < 1 but it's fine for now

            renderer.sharedMaterial.mainTextureScale = new Vector2((1 / aspectRatio) * screenScale.x, 1);
            renderer.sharedMaterial.mainTextureOffset = new Vector2((1 - (1 / aspectRatio) * screenScale.x) / 2, 0);
        }
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.Log($"Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
    }

    private void OnDestroy()
    {
        _imageDownloader.Dispose();
    }
}