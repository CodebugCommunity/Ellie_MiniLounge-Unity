
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using UdonSharp;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class OkPanel : UdonSharpBehaviour
{
    public TextMeshProUGUI ownerLabel;
    public TextMeshProUGUI colorSliderLabel;
    public TextMeshProUGUI pickupLabel;
    public TextMeshProUGUI layerLabel;
    public TextMeshProUGUI imageSizeLabel;
    public TextMeshProUGUI layerTitle;
    public TextMeshProUGUI layerSwapLabel;
    public TextMeshProUGUI layerBlendLabel;
    public TextMeshProUGUI layerOpacityLabel;
    public GameObject[] layerButtonHighlights;
    public GameObject[] colorModes;
    public GameObject[] layerEyeOnIcons;
    public GameObject[] layerEyeOffIcons;
    public GameObject[] layerBlendButtons;
    public Slider colorSliderUI;
    public Slider layerOpacitySlider;
    public Material hueMaterial;
    public Material saturationMaterial;
    public Material lightnessMaterial;
    public VRC_Pickup panelPickup;
    public VRC_Pickup canvasPickup;
    public VRC_Pickup[] toolPickups;
    public GameObject[] pickupModels;
    public Animator[] styleBlipAnimators;
    public Animator[] blipAnimators;
    public TextMeshProUGUI[] blipLabels;
    public RectTransform[] blipTransforms;
    public RectTransform[] otherBlipTransforms;
    public GameObject[] otherStyleBlips;
    public Transform imageSizeTransform;
    public Camera[] imageSizeCameras;
    public Camera imageSizeSyncCamera;
    public OkSync synchronizer;
    public GameObject[] layerCameras;
    public GameObject[] layerUIObjects;
    public GameObject[] layerDisplayObjects;
    public GameObject noBackgroundObject;
    public Material[] layerMaterials;
    public Material[] thumbnailMaterials;
    public GameObject fullErase;
    public GameObject mergeButtonCover;
    public GameObject[] mergeCameras;
    public Slider eraseSlider;
    public Slider mergeSlider;
    public Slider copyCameraSlider;
    public OkCamera cameraPickup;
    public GameObject screenshotCameraCopyDisplay;
    public GameObject[] layerSwaps0;
    public GameObject[] layerSwaps1;
    public GameObject[] layerSwaps2;
    public GameObject[] layerCameraSwaps0;
    public GameObject[] layerCameraSwaps1;
    public GameObject[] layerCameraSwaps2;
    public GameObject[] thumbnails;
    private float eraseConfirmTimer;
    private float mergeConfirmTimer;
    private float copyCameraConfirmTimer;
    private int mergeTimer;
    private float autoDisableVisibilityTimer = -1.0f;
    private float fullEraseTimer = -1.0f;
    private float copyCameraTimer = -1.0f;
    private int colorSliderValueBuffer = -1;
    private int layerOpacitySliderValueBuffer = -1;
    private int layerButtonBuffer = -1;
    private int sizeButtonsBuffer = -1;
    private bool LocalPlayerOwnsThis { get { return Networking.IsOwner(Networking.LocalPlayer, gameObject); } }
    private bool LocalPlayerHasPermission { get { return OwnerLock ? LocalPlayerOwnsThis : true; } }
    [UdonSynced, FieldChangeCallback(nameof(OwnerLock))] private bool _ownerLock; public bool OwnerLock { get { return _ownerLock; } set { _ownerLock = value; OwnerLockUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(PickupLock))] private bool _pickupLock = true; public bool PickupLock { get { return _pickupLock; } set { _pickupLock = value; PickupsUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(ColorMode))] public int _colorMode; public int ColorMode { get { return _colorMode; } set { _colorMode = value; ColorModeUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(ColorSlider))] public int _colorSlider = 8; public int ColorSlider { get { return _colorSlider; } set { _colorSlider = value; ColorSliderUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(ImageSize))] public int _imageSize = 0; public int ImageSize { get { return _imageSize; } set { _imageSize = value; ImageSizeUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(ActiveLayer))] private int _activeLayer = 0; public int ActiveLayer { get { return _activeLayer; } set { _activeLayer = value; ActiveLayerUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(ShowLayerUI))] private bool _showLayerUI = false; public bool ShowLayerUI { get { return _showLayerUI; } set { _showLayerUI = value; ShowLayerUIUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Layer0Visible))] private bool _layer0Visible = true; public bool Layer0Visible { get { return _layer0Visible; } set { _layer0Visible = value; LayerVisibleUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Layer1Visible))] private bool _layer1Visible = true; public bool Layer1Visible { get { return _layer1Visible; } set { _layer1Visible = value; LayerVisibleUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Layer2Visible))] private bool _layer2Visible = true; public bool Layer2Visible { get { return _layer2Visible; } set { _layer2Visible = value; LayerVisibleUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Layer0Opacity))] private float _layer0Opacity = 1.0f; public float Layer0Opacity { get { return _layer0Opacity; } set { _layer0Opacity = value; LayerOpacityUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Layer1Opacity))] private float _layer1Opacity = 1.0f; public float Layer1Opacity { get { return _layer1Opacity; } set { _layer1Opacity = value; LayerOpacityUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Layer2Opacity))] private float _layer2Opacity = 1.0f; public float Layer2Opacity { get { return _layer2Opacity; } set { _layer2Opacity = value; LayerOpacityUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Layer0Blend))] private int _layer0Blend; public int Layer0Blend { get { return _layer0Blend; } set { _layer0Blend = value; LayerBlendUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Layer1Blend))] private int _layer1Blend; public int Layer1Blend { get { return _layer1Blend; } set { _layer1Blend = value; LayerBlendUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Layer2Blend))] private int _layer2Blend; public int Layer2Blend { get { return _layer2Blend; } set { _layer2Blend = value; LayerBlendUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(LayerSwap0))] private int _layerSwap0 = 0; public int LayerSwap0 { get { return _layerSwap0; } set { _layerSwap0 = value; LayerSwapUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(LayerSwap1))] private int _layerSwap1 = 1; public int LayerSwap1 { get { return _layerSwap1; } set { _layerSwap1 = value; LayerSwapUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(LayerSwap2))] private int _layerSwap2 = 2; public int LayerSwap2 { get { return _layerSwap2; } set { _layerSwap2 = value; LayerSwapUpdate(); } }

    public void Start()
    {
        OwnerLock = _ownerLock;
        PickupLock = _pickupLock;
        ColorMode = _colorMode;
        ColorSlider = _colorSlider;
        ImageSize = _imageSize;
        ActiveLayer = _activeLayer;
        ShowLayerUI = _showLayerUI;
        Layer0Visible = _layer0Visible;
        Layer1Visible = _layer1Visible;
        Layer2Visible = _layer2Visible;
        Layer0Opacity = _layer0Opacity;
        Layer1Opacity = _layer1Opacity;
        Layer2Opacity = _layer2Opacity;
        Layer0Blend = _layer0Blend;
        Layer1Blend = _layer1Blend;
        Layer2Blend = _layer2Blend;
        LayerSwap0 = _layerSwap0;
        LayerSwap1 = _layerSwap1;
        LayerSwap2 = _layerSwap2;
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        OwnerLockUpdate();

        if (LocalPlayerOwnsThis)
        {
            OwnerLock = false;

            if (colorSliderValueBuffer != -1)
            {
                Button_ColorSlider(colorSliderValueBuffer);
            }

            if (layerOpacitySliderValueBuffer != -1)
            {
                Button_LayerOpacitySlider(layerOpacitySliderValueBuffer);
            }

            if (layerButtonBuffer != -1)
            {
                Button_Layers();
            }

            if (sizeButtonsBuffer != -1)
            {
                if (sizeButtonsBuffer > 0)
                {
                    Button_SizePlus();
                }
                else
                {
                    Button_SizeMinus();
                }
            }
        }

        colorSliderValueBuffer = -1;
        layerOpacitySliderValueBuffer = -1;
        layerButtonBuffer = -1;
        sizeButtonsBuffer = -1;
    }

    public void OwnerLockUpdate()
    {
        if (LocalPlayerOwnsThis)
        {
            ownerLabel.text = OwnerLock ? "Unlock\nOwnership" : "Lock\nOwnership";
        }
        else
        {
            ownerLabel.text = OwnerLock ? "Locked\nBy Owner" : "Take\nOwnership";
        }

        PickupsUpdate();
    }

    public void PickupsUpdate()
    {
#if UNITY_ANDROID
        var canPickupPanels = false;
#else
        var canPickupPanels = LocalPlayerHasPermission && !PickupLock;
#endif
        panelPickup.pickupable = canPickupPanels;
        canvasPickup.pickupable = canPickupPanels;
        if (!canPickupPanels)
        {
            panelPickup.Drop();
            canvasPickup.Drop();
        }

        for (int i = 0; i < pickupModels.Length; i++)
        {
            pickupModels[i].SetActive(canPickupPanels);
        }

#if UNITY_ANDROID
        var canPickupTools = false;
#else
        var canPickupTools = LocalPlayerHasPermission;
#endif
        for (int i = 0; i < toolPickups.Length; i++)
        {
            toolPickups[i].pickupable = canPickupTools;

            if (i == 3)
            {
                toolPickups[i].pickupable = canPickupTools && ShowLayerUI;
            }

            if (!canPickupTools)
            {
                toolPickups[i].Drop();
            }
        }

        synchronizer.pickupable = canPickupTools;
        if (synchronizer.DisplayIsActive)
        {
            synchronizer.displayPickup.pickupable = canPickupTools;
        }

        pickupLabel.text = PickupLock ? "Enable\nPickup" : "Disable\nPickup";
    }

    public void ImageSizeUpdate()
    {
        var newScale = 1.0f + ImageSize * (ImageSize < 0 ? 0.1f : 0.2f);
        if (ImageSize > 5)
        {
            newScale += (ImageSize - 5) * 0.4f;
        }

        imageSizeTransform.localScale = new Vector3(newScale, newScale, 1.0f);
        for (int i = 0; i < imageSizeCameras.Length; i++)
        {
            imageSizeCameras[i].orthographicSize = newScale * 0.2f;
        }

        imageSizeSyncCamera.orthographicSize = newScale * 0.2f;

        var newText = "Image Size:\nNormal";
        if (ImageSize > 0)
        {
            newText = "Image Size:\n+" + ImageSize;
        }
        else if (ImageSize < 0)
        {
            newText = "Image Size:\n" + ImageSize;
        }
        imageSizeLabel.text = newText;

        synchronizer.ImageSizeUpdate(newScale);
    }

    public void ShowLayerUIUpdate()
    {
        for (int i = 0; i < layerUIObjects.Length; i++)
        {
            layerUIObjects[i].SetActive(ShowLayerUI);
        }
        layerLabel.text = ShowLayerUI ? "Disable\nLayers" : "Enable\nLayers";
        cameraPickup.SetVisible(ShowLayerUI, LocalPlayerHasPermission);
    }

    public void ThumbnailUpdate()
    {
        var activeRT = 0;
        switch (ActiveLayer)
        {
            case 0: activeRT = LayerSwap0; break;
            case 1: activeRT = LayerSwap1; break;
            case 2: activeRT = LayerSwap2; break;
        }
        for (int i = 0; i < 3; i++)
        {
            thumbnails[i].SetActive(activeRT == i);
        }
    }

    public void ActiveLayerUpdate()
    {
        layerCameras[0].SetActive(ActiveLayer == 0);
        layerCameras[1].SetActive(ActiveLayer == 1);
        layerCameras[2].SetActive(ActiveLayer == 2);

        layerButtonHighlights[0].SetActive(ActiveLayer == 0);
        layerButtonHighlights[1].SetActive(ActiveLayer == 1);
        layerButtonHighlights[2].SetActive(ActiveLayer == 2);

        switch (ActiveLayer)
        {
            case 0: layerTitle.text = "Drawing on Background"; layerSwapLabel.text = "Swap with\nLayer 1"; break;
            case 1: layerTitle.text = "Drawing on Layer 1"; layerSwapLabel.text = "Swap with\nLayer 2"; break;
            case 2: layerTitle.text = "Drawing on Layer 2"; layerSwapLabel.text = "Swap with\nLayer 1"; break;
        }

        layerBlendButtons[0].SetActive(ActiveLayer > 0);
        layerBlendButtons[1].SetActive(ActiveLayer > 0);
        mergeButtonCover.SetActive(ActiveLayer > 0);

        LayerVisibleUpdate();
        LayerOpacityUpdate();
        LayerBlendUpdate();
        ThumbnailUpdate();
    }

    public void LayerVisibleUpdate()
    {
        layerDisplayObjects[0].SetActive(Layer0Visible);
        layerDisplayObjects[1].SetActive(Layer1Visible);
        layerDisplayObjects[2].SetActive(Layer2Visible);

        noBackgroundObject.SetActive(!Layer0Visible);

        layerEyeOnIcons[0].SetActive(Layer0Visible);
        layerEyeOnIcons[1].SetActive(Layer1Visible);
        layerEyeOnIcons[2].SetActive(Layer2Visible);

        layerEyeOffIcons[0].SetActive(!Layer0Visible);
        layerEyeOffIcons[1].SetActive(!Layer1Visible);
        layerEyeOffIcons[2].SetActive(!Layer2Visible);
    }

    public void LayerOpacityUpdate()
    {
        var activeLayerOpacity = 1.0f;
        switch (ActiveLayer)
        {
            case 0: activeLayerOpacity = Layer0Opacity; break;
            case 1: activeLayerOpacity = Layer1Opacity; break;
            case 2: activeLayerOpacity = Layer2Opacity; break;
        }

        layerOpacitySlider.value = activeLayerOpacity * 100.0f;
        layerOpacityLabel.text = "Opacity " + Mathf.RoundToInt(activeLayerOpacity * 100.0f);

        layerMaterials[0].SetFloat("_Multiply", Layer0Opacity);
        layerMaterials[1].SetFloat("_Multiply", Layer0Opacity);
        layerMaterials[2].SetFloat("_Multiply", Layer0Opacity);

        layerMaterials[3].SetFloat("_Opacity", Layer1Opacity);
        layerMaterials[4].SetFloat("_Opacity", Layer1Opacity);
        layerMaterials[5].SetFloat("_Opacity", Layer1Opacity);

        layerMaterials[6].SetFloat("_Opacity", Layer2Opacity);
        layerMaterials[7].SetFloat("_Opacity", Layer2Opacity);
        layerMaterials[8].SetFloat("_Opacity", Layer2Opacity);

        thumbnailMaterials[0].SetFloat("_Multiply", activeLayerOpacity);
        thumbnailMaterials[1].SetFloat("_Multiply", activeLayerOpacity);
        thumbnailMaterials[2].SetFloat("_Multiply", activeLayerOpacity);
    }

    public void LayerBlendUpdate()
    {
        var activeLayerBlendMode = 0;
        switch (ActiveLayer)
        {
            case 0: activeLayerBlendMode = Layer0Blend; break;
            case 1: activeLayerBlendMode = Layer1Blend; break;
            case 2: activeLayerBlendMode = Layer2Blend; break;
        }

        var blendModeName = "";
        switch (activeLayerBlendMode)
        {
            case 0: blendModeName = "Normal"; break;
            case 1: blendModeName = "Lighten"; break;
            case 2: blendModeName = "Darken"; break;
            case 3: blendModeName = "Invert"; break;
        }

        layerBlendLabel.text = "Blend Mode:\n" + (ActiveLayer == 0 ? "Background" : blendModeName);

        // UnityEngine.Rendering.BlendMode
        //   Zero = 0
        //   One = 1
        //   DstColor = 2
        //   SrcColor = 3
        //   OneMinusDstColor = 4
        //   SrcAlpha = 5
        //   OneMinusSrcColor = 6
        //   DstAlpha = 7
        //   OneMinusDstAlpha = 8
        //   SrcAlphaSaturate = 9
        //   OneMinusSrcAlpha = 10

        if (Layer1Blend == 0)
        {
            layerMaterials[3].SetInt("_MySrcMode", 5);
            layerMaterials[3].SetInt("_MyDstMode", 10);
            layerMaterials[3].SetFloat("_Invert", 0.0f);
            layerMaterials[4].SetInt("_MySrcMode", 5);
            layerMaterials[4].SetInt("_MyDstMode", 10);
            layerMaterials[4].SetFloat("_Invert", 0.0f);
            layerMaterials[5].SetInt("_MySrcMode", 5);
            layerMaterials[5].SetInt("_MyDstMode", 10);
            layerMaterials[5].SetFloat("_Invert", 0.0f);
        }
        else if (Layer1Blend == 1)
        {
            layerMaterials[3].SetInt("_MySrcMode", 1);
            layerMaterials[3].SetInt("_MyDstMode", 1);
            layerMaterials[3].SetFloat("_Invert", 0.0f);
            layerMaterials[4].SetInt("_MySrcMode", 1);
            layerMaterials[4].SetInt("_MyDstMode", 1);
            layerMaterials[4].SetFloat("_Invert", 0.0f);
            layerMaterials[5].SetInt("_MySrcMode", 1);
            layerMaterials[5].SetInt("_MyDstMode", 1);
            layerMaterials[5].SetFloat("_Invert", 0.0f);
        }
        else if (Layer1Blend == 2)
        {
            layerMaterials[3].SetInt("_MySrcMode", 2);
            layerMaterials[3].SetInt("_MyDstMode", 0);
            layerMaterials[3].SetFloat("_Invert", 0.0f);
            layerMaterials[4].SetInt("_MySrcMode", 2);
            layerMaterials[4].SetInt("_MyDstMode", 0);
            layerMaterials[4].SetFloat("_Invert", 0.0f);
            layerMaterials[5].SetInt("_MySrcMode", 2);
            layerMaterials[5].SetInt("_MyDstMode", 0);
            layerMaterials[5].SetFloat("_Invert", 0.0f);
        }
        else if (Layer1Blend == 3)
        {
            layerMaterials[3].SetInt("_MySrcMode", 5);
            layerMaterials[3].SetInt("_MyDstMode", 10);
            layerMaterials[3].SetFloat("_Invert", 1.0f);
            layerMaterials[4].SetInt("_MySrcMode", 5);
            layerMaterials[4].SetInt("_MyDstMode", 10);
            layerMaterials[4].SetFloat("_Invert", 1.0f);
            layerMaterials[5].SetInt("_MySrcMode", 5);
            layerMaterials[5].SetInt("_MyDstMode", 10);
            layerMaterials[5].SetFloat("_Invert", 1.0f);
        }

        if (Layer2Blend == 0)
        {
            layerMaterials[6].SetInt("_MySrcMode", 5);
            layerMaterials[6].SetInt("_MyDstMode", 10);
            layerMaterials[6].SetFloat("_Invert", 0.0f);
            layerMaterials[7].SetInt("_MySrcMode", 5);
            layerMaterials[7].SetInt("_MyDstMode", 10);
            layerMaterials[7].SetFloat("_Invert", 0.0f);
            layerMaterials[8].SetInt("_MySrcMode", 5);
            layerMaterials[8].SetInt("_MyDstMode", 10);
            layerMaterials[8].SetFloat("_Invert", 0.0f);
        }
        else if (Layer2Blend == 1)
        {
            layerMaterials[6].SetInt("_MySrcMode", 1);
            layerMaterials[6].SetInt("_MyDstMode", 1);
            layerMaterials[6].SetFloat("_Invert", 0.0f);
            layerMaterials[7].SetInt("_MySrcMode", 1);
            layerMaterials[7].SetInt("_MyDstMode", 1);
            layerMaterials[7].SetFloat("_Invert", 0.0f);
            layerMaterials[8].SetInt("_MySrcMode", 1);
            layerMaterials[8].SetInt("_MyDstMode", 1);
            layerMaterials[8].SetFloat("_Invert", 0.0f);
        }
        else if (Layer2Blend == 2)
        {
            layerMaterials[6].SetInt("_MySrcMode", 2);
            layerMaterials[6].SetInt("_MyDstMode", 0);
            layerMaterials[6].SetFloat("_Invert", 0.0f);
            layerMaterials[7].SetInt("_MySrcMode", 2);
            layerMaterials[7].SetInt("_MyDstMode", 0);
            layerMaterials[7].SetFloat("_Invert", 0.0f);
            layerMaterials[8].SetInt("_MySrcMode", 2);
            layerMaterials[8].SetInt("_MyDstMode", 0);
            layerMaterials[8].SetFloat("_Invert", 0.0f);
        }
        else if (Layer2Blend == 3)
        {
            layerMaterials[6].SetInt("_MySrcMode", 5);
            layerMaterials[6].SetInt("_MyDstMode", 10);
            layerMaterials[6].SetFloat("_Invert", 1.0f);
            layerMaterials[7].SetInt("_MySrcMode", 5);
            layerMaterials[7].SetInt("_MyDstMode", 10);
            layerMaterials[7].SetFloat("_Invert", 1.0f);
            layerMaterials[8].SetInt("_MySrcMode", 5);
            layerMaterials[8].SetInt("_MyDstMode", 10);
            layerMaterials[8].SetFloat("_Invert", 1.0f);
        }
    }

    public void LayerSwapUpdate()
    {
        for (int i = 0; i < 3; i++)
        {
            layerSwaps0[i].SetActive(LayerSwap0 == i);
            layerSwaps1[i].SetActive(LayerSwap1 == i);
            layerSwaps2[i].SetActive(LayerSwap2 == i);

            layerCameraSwaps0[i].SetActive(LayerSwap0 == i);
            layerCameraSwaps1[i].SetActive(LayerSwap1 == i);
            layerCameraSwaps2[i].SetActive(LayerSwap2 == i);
        }
        ThumbnailUpdate();
    }

    public void Button_Owner()
    {
        if (LocalPlayerOwnsThis)
        {
            OwnerLock = !OwnerLock;
        }
        else if (!OwnerLock)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_Pickup()
    {
        if (LocalPlayerOwnsThis)
        {
            PickupLock = !PickupLock;
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_Pickup));
        }
    }

    public void Button_Layers()
    {
        if (LocalPlayerOwnsThis)
        {
            ShowLayerUI = !ShowLayerUI;
        }
        else if (LocalPlayerHasPermission)
        {
            layerButtonBuffer = 1;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_Layer0()
    {
        if (LocalPlayerOwnsThis)
        {
            ActiveLayer = 0;
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_Layer0));
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_Layer1()
    {
        if (LocalPlayerOwnsThis)
        {
            ActiveLayer = 1;
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_Layer1));
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_Layer2()
    {
        if (LocalPlayerOwnsThis)
        {
            ActiveLayer = 2;
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_Layer2));
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_LayerEye2()
    {
        if (LocalPlayerOwnsThis)
        {
            Layer2Visible = !Layer2Visible;
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_LayerEye2));
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_LayerEye1()
    {
        if (LocalPlayerOwnsThis)
        {
            Layer1Visible = !Layer1Visible;
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_LayerEye1));
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_LayerEye0()
    {
        if (LocalPlayerOwnsThis)
        {
            Layer0Visible = !Layer0Visible;
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_LayerEye0));
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_LayerSwap()
    {
        if (LocalPlayerOwnsThis)
        {
            if (ActiveLayer == 0)
            {
                var temp = LayerSwap0;
                LayerSwap0 = LayerSwap1;
                LayerSwap1 = temp;
            }
            else
            {
                var temp = LayerSwap1;
                LayerSwap1 = LayerSwap2;
                LayerSwap2 = temp;
            }
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_LayerSwap));
        }
    }

    public void Button_LayerBlendLeft()
    {
        if (LocalPlayerOwnsThis)
        {
            switch (ActiveLayer)
            {
                case 0: Layer0Blend = (Layer0Blend + 3) % 4; break;
                case 1: Layer1Blend = (Layer1Blend + 3) % 4; break;
                case 2: Layer2Blend = (Layer2Blend + 3) % 4; break;
            }
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_LayerBlendLeft));
        }
    }

    public void Button_LayerBlendRight()
    {
        if (LocalPlayerOwnsThis)
        {
            switch (ActiveLayer)
            {
                case 0: Layer0Blend = (Layer0Blend + 1) % 4; break;
                case 1: Layer1Blend = (Layer1Blend + 1) % 4; break;
                case 2: Layer2Blend = (Layer2Blend + 1) % 4; break;
            }
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_LayerBlendRight));
        }
    }

    public void Button_LayerErase()
    {
        if (LocalPlayerOwnsThis)
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EraseLayer));
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_LayerErase));
        }
    }

    public void Button_LayerEraseStartPress()
    {
        eraseConfirmTimer = 0.1f;
    }

    public void Button_LayerEraseHold()
    {
        eraseConfirmTimer += Time.deltaTime * 3.4f;
        eraseSlider.gameObject.SetActive(true);
    }

    public void EraseLayer()
    {
        fullEraseTimer = 0.25f;
        fullErase.SetActive(true);
    }

    public void Button_LayerCopyCamera()
    {
        if (LocalPlayerOwnsThis)
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(CopyCamera));
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_LayerCopyCamera));
        }
    }

    public void CopyCamera()
    {
        copyCameraTimer = 0.25f;
        screenshotCameraCopyDisplay.SetActive(true);
    }

    public void Button_LayerCopyCameraStartPress()
    {
        copyCameraConfirmTimer = 0.1f;
    }

    public void Button_LayerCopyCameraHold()
    {
        copyCameraConfirmTimer += Time.deltaTime * 3.4f;
        copyCameraSlider.gameObject.SetActive(true);
    }

    public void Button_LayerMerge()
    {
        if (LocalPlayerOwnsThis)
        {
            autoDisableVisibilityTimer = 0.65f;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Merge));
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_LayerMerge));
        }
    }

    public void Button_LayerMergeStartPress()
    {
        mergeConfirmTimer = 0.1f;
    }

    public void Button_LayerMergeHold()
    {
        mergeConfirmTimer += Time.deltaTime * 3.4f;
        mergeSlider.gameObject.SetActive(true);
    }

    public void Merge()
    {
        mergeTimer = 2;
    }

    public void Button_LayerOpacitySlider(int newValue)
    {
        if (LocalPlayerOwnsThis)
        {
            switch (ActiveLayer)
            {
                case 0: Layer0Opacity = newValue / 100.0f; break;
                case 1: Layer1Opacity = newValue / 100.0f; break;
                case 2: Layer2Opacity = newValue / 100.0f; break;
            }
        }
        else if (LocalPlayerHasPermission)
        {
            layerOpacitySliderValueBuffer = newValue;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_LayerOpacitySliderDrag(int newValue)
    {
        if (LocalPlayerOwnsThis)
        {
            switch (ActiveLayer)
            {
                case 0: Layer0Opacity = newValue / 100.0f; break;
                case 1: Layer1Opacity = newValue / 100.0f; break;
                case 2: Layer2Opacity = newValue / 100.0f; break;
            }
        }
    }

    public void Button_Sync()
    {
        synchronizer.Activate();
    }

    public void Button_SizeMinus()
    {
        if (LocalPlayerOwnsThis)
        {
            var newSize = Mathf.Clamp(ImageSize - 1, -5, 10);
            ImageSize = newSize;
        }
        else if (LocalPlayerHasPermission)
        {
            sizeButtonsBuffer = -100;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_SizePlus()
    {
        if (LocalPlayerOwnsThis)
        {
            var newSize = Mathf.Clamp(ImageSize + 1, -5, 10);
            ImageSize = newSize;
        }
        else if (LocalPlayerHasPermission)
        {
            sizeButtonsBuffer = 100;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void PlayStyleBlipAnimation(int index)
    {
        styleBlipAnimators[index].SetTrigger("Show");
    }

    public void PlayRangeBlipAnimation(int index, int value)
    {
        blipAnimators[index].SetTrigger("Show");
        var p = blipTransforms[index].anchoredPosition;
        p.x = value * 2.97f;
        blipTransforms[index].anchoredPosition = p;
        blipLabels[index].text = value.ToString();
    }

    public void ShowOtherBlip(int index, int value)
    {
        otherBlipTransforms[index].gameObject.SetActive(true);
        var p = otherBlipTransforms[index].anchoredPosition;
        p.x = value * 2.97f;
        otherBlipTransforms[index].anchoredPosition = p;

    }

    public void HideOtherBlip(int index)
    {
        otherBlipTransforms[index].gameObject.SetActive(false);
    }

    public void ShowStyleBlip(int index)
    {
        for (int i = 0; i < otherStyleBlips.Length; i++)
        {
            otherStyleBlips[i].SetActive(i == index);
        }
    }

    public void HideStyleBlips()
    {
        for (int i = 0; i < otherStyleBlips.Length; i++)
        {
            otherStyleBlips[i].SetActive(false);
        }
    }

    public void Button_ColorMode()
    {
        if (LocalPlayerOwnsThis)
        {
            var newColorMode = (ColorMode + 1) % 3;
            ColorMode = newColorMode;
        }
        else if (LocalPlayerHasPermission)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Button_ColorMode));
        }
    }

    public void Button_ColorSlider(int newValue)
    {
        if (LocalPlayerOwnsThis)
        {
            ColorSlider = newValue;
        }
        else if (LocalPlayerHasPermission)
        {
            colorSliderValueBuffer = newValue;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public void Button_ColorSliderDrag(int newValue)
    {
        if (LocalPlayerOwnsThis)
        {
            ColorSlider = newValue;
        }
    }

    public void ColorModeUpdate()
    {
        colorModes[0].SetActive(ColorMode == 0);
        colorModes[1].SetActive(ColorMode == 1);
        colorModes[2].SetActive(ColorMode == 2);

        ColorSliderUpdate();
    }

    public void ColorSliderUpdate()
    {
        colorSliderUI.value = ColorSlider;
        var normalizedValue = ColorSlider / 100.0f;
        hueMaterial.SetFloat("_Hue", normalizedValue);
        saturationMaterial.SetFloat("_Saturation", normalizedValue);
        lightnessMaterial.SetFloat("_Lightness", normalizedValue);

        if (ColorMode == 0)
        {
            colorSliderLabel.text = "Hue " + ColorSlider;
        }
        else if (ColorMode == 1)
        {
            colorSliderLabel.text = "Saturation " + ColorSlider;
        }
        else
        {
            colorSliderLabel.text = "Lightness " + ColorSlider;
        }
    }

    public void Update()
    {
        // Erase layer
        if (fullEraseTimer > 0.0f)
        {
            fullEraseTimer -= Time.deltaTime;
            if (fullEraseTimer <= 0.0f)
            {
                fullEraseTimer = -1.0f;
                fullErase.SetActive(false);
            }
        }

        // Copy camera
        if (copyCameraTimer > 0.0f)
        {
            copyCameraTimer -= Time.deltaTime;
            if (copyCameraTimer <= 0.0f)
            {
                copyCameraTimer = -1.0f;
                screenshotCameraCopyDisplay.SetActive(false);
            }
        }

        // Merge sequence
        if (mergeTimer == 1)
        {
            mergeCameras[LayerSwap0].SetActive(false);
            mergeTimer = 0;
        }
        if (mergeTimer == 2)
        {
            mergeCameras[LayerSwap0].SetActive(true);
            mergeTimer = 1;
        }

        // Confirm erase
        if (eraseConfirmTimer >= 0.0f)
        {
            if (eraseConfirmTimer > 1.0f && eraseConfirmTimer < 99.0f)
            {
                Button_LayerErase();
                eraseConfirmTimer = 100.0f;
                eraseSlider.value = 0.0f;
                eraseSlider.gameObject.SetActive(false);
            }

            if (eraseConfirmTimer < 99.0f)
            {
                eraseConfirmTimer -= Time.deltaTime * 2.2f;
                eraseSlider.value = eraseConfirmTimer;
                if (eraseConfirmTimer <= 0.0f)
                {
                    eraseSlider.gameObject.SetActive(false);
                    eraseConfirmTimer = 0.0f;
                }
            }
        }

        // Confirm merge
        if (mergeConfirmTimer >= 0.0f)
        {
            if (mergeConfirmTimer > 1.0f && mergeConfirmTimer < 99.0f)
            {
                Button_LayerMerge();
                mergeConfirmTimer = 100.0f;
                mergeSlider.value = 0.0f;
                mergeSlider.gameObject.SetActive(false);
            }

            if (mergeConfirmTimer < 99.0f)
            {
                mergeConfirmTimer -= Time.deltaTime * 2.2f;
                mergeSlider.value = mergeConfirmTimer;
                if (mergeConfirmTimer <= 0.0f)
                {
                    mergeSlider.gameObject.SetActive(false);
                    mergeConfirmTimer = 0.0f;
                }
            }
        }

        // Confirm copy camera
        if (copyCameraConfirmTimer >= 0.0f)
        {
            if (copyCameraConfirmTimer > 1.0f && copyCameraConfirmTimer < 99.0f)
            {
                Button_LayerCopyCamera();
                copyCameraConfirmTimer = 100.0f;
                copyCameraSlider.value = 0.0f;
                copyCameraSlider.gameObject.SetActive(false);
            }

            if (copyCameraConfirmTimer < 99.0f)
            {
                copyCameraConfirmTimer -= Time.deltaTime * 2.2f;
                copyCameraSlider.value = copyCameraConfirmTimer;
                if (copyCameraConfirmTimer <= 0.0f)
                {
                    copyCameraSlider.gameObject.SetActive(false);
                    copyCameraConfirmTimer = 0.0f;
                }
            }
        }

        // Auto disable layers after merge
        if (autoDisableVisibilityTimer > 0.0f)
        {
            autoDisableVisibilityTimer -= Time.deltaTime;
            if (autoDisableVisibilityTimer <= 0.0f)
            {
                autoDisableVisibilityTimer = -1.0f;
                if (Layer1Visible)
                {
                    Button_LayerEye1();
                }
                if (Layer2Visible)
                {
                    Button_LayerEye2();
                }
            }
        }
    }
}
