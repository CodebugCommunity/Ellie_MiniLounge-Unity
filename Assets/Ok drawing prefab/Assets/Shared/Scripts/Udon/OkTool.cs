
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using UdonSharp;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class OkTool : UdonSharpBehaviour
{
    public OkPanel panel;
    public Collider pointerCollider;
    public Collider layerUICollider;
    public Collider ownerButtonCollider;
    public Collider pickupButtonCollider;
    public Collider layerButtonCollider;
    public Collider syncButtonCollider;
    public Collider sizeMinusButtonCollider;
    public Collider sizePlusButtonCollider;
    public Collider style0ButtonCollider;
    public Collider style1ButtonCollider;
    public Collider style2ButtonCollider;
    public Collider style3ButtonCollider;
    public Collider style4ButtonCollider;
    public Collider style5ButtonCollider;
    public Collider sizeRangeCollider;
    public Collider softnessRangeCollider;
    public Collider opacityRangeCollider;
    public Collider colorModeButtonCollider;
    public Collider colorSliderCollider;
    public Collider layer2Collider;
    public Collider layer1Collider;
    public Collider layer0Collider;
    public Collider layerEye2Collider;
    public Collider layerEye1Collider;
    public Collider layerEye0Collider;
    public Collider layerSwapCollider;
    public Collider layerBlendLeftCollider;
    public Collider layerBlendRightCollider;
    public Collider layerEraseCollider;
    public Collider layerCopyCameraCollider;
    public Collider layerMergeCollider;
    public Collider layerOpacitySliderCollider;
    public GameObject pointerSet;
    public GameObject paintingSet;
    public Transform pointerLine;
    public Transform pointerDot;
    public GameObject touchPaintSet;
    public GameObject touchBlendSet;
    public Camera touchColorPicker;
    public Animator[] touchProjectorAnimators;
    public Collider canvasCollider;
    public Collider paletteCollider;
    public GameObject pointerGuide;
    public GameObject pointerColorPickerGuide;
    public Transform pointerProjectorContainer;
    public GameObject pointerColorPickerCamera;
    public Transform brushHeadModels;
    public Material[] brushOpacityMaterials;
    public GameObject pointerBrushObjects;
    public GameObject touchBrushObjects;
    public GameObject colorInitializer;
    public GameObject[] styleHeadModels;
    public GameObject[] touchStyleObjects;
    public GameObject[] touchStyleBlendObjects;
    public Transform raycastStart;
    private Animator pointerAnim;
    private Animator[] projectorAnimators;
    private Transform[] projectorTransforms;
    private GameObject[] pointerPaintingObjects;
    private GameObject[] pointerErasingObjects;
    private int projectorsOn;
    private int pointerProjectorCount;
    private Vector3 lastPaintPosition;
    private Vector3 lastHitPosition;
    private bool lastRayMissed = true;
    private bool useDown;
    private bool firstFrameUseDown;
    private bool pointerColorPickerActive;
    private bool uiPointerActive;
    private float step;
    private float distanceAccumulator;
    private float initTimer;
    private bool usingColorSlider;
    private bool usingLayerOpacitySlider;
    private bool usingSizeRange;
    private bool usingSoftnessRange;
    private bool usingOpacityRange;
    private bool usingEraseButton;
    private bool usingMergeButton;
    private bool usingCopyCameraButton;
    private Transform followers;
    private bool activatedBlips;
    private readonly float uiPointerRayRange = 3.0f;
    private readonly float pointerBrushRayRange = 10.0f;
    private bool TouchStyleIsActive { get { return Style != 3 && Style != 5; } }
    private bool PointerStyleIsActive { get { return !TouchStyleIsActive; } }
    private bool LocalPlayerOwnsThis { get { return Networking.IsOwner(Networking.LocalPlayer, gameObject); } }
    [UdonSynced, FieldChangeCallback(nameof(Style))] public int _style; public int Style { get { return _style; } set { _style = value; StyleUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Size))] public float _size; public float Size { get { return _size; } set { _size = value; SizeUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Softness))] public float _softness; public float Softness { get { return _softness; } set { _softness = value; SoftnessUpdate(); } }
    [UdonSynced, FieldChangeCallback(nameof(Opacity))] public float _opacity; public float Opacity { get { return _opacity; } set { _opacity = value; OpacityUpdate(); } }

    public void Start()
    {
        pointerAnim = (Animator)pointerSet.transform.parent.GetComponent("Animator");
        pointerProjectorCount = pointerProjectorContainer.childCount;
        projectorTransforms = new Transform[pointerProjectorCount];
        projectorAnimators = new Animator[pointerProjectorCount];
        pointerPaintingObjects = new GameObject[pointerProjectorCount];
        pointerErasingObjects = new GameObject[pointerProjectorCount];
        for (int i = 0; i < pointerProjectorCount; i++)
        {
            var child = pointerProjectorContainer.GetChild(i);
            projectorTransforms[i] = child;
            projectorAnimators[i] = child.GetComponent<Animator>();
            pointerPaintingObjects[i] = child.GetChild(0).gameObject;
            pointerErasingObjects[i] = child.GetChild(1).gameObject;
        }

        var brushContainer = transform.parent;
        for (int i = 0; i < pointerProjectorCount; i++)
        {
            projectorTransforms[i].SetParent(brushContainer);
        }
        pointerGuide.transform.SetParent(brushContainer);

        var pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        pickup.AutoHold = VRC_Pickup.AutoHoldMode.Yes;
        if (Networking.LocalPlayer != null && Networking.LocalPlayer.IsUserInVR() == false)
        {
            pickup.orientation = VRC_Pickup.PickupOrientation.Grip;
        }

        for (int i = 0; i < projectorAnimators.Length; i++)
        {
            projectorAnimators[i].keepAnimatorStateOnDisable = true;
        }
        for (int i = 0; i < touchProjectorAnimators.Length; i++)
        {
            touchProjectorAnimators[i].keepAnimatorStateOnDisable = true;
        }
        touchBlendSet.SetActive(false);

        followers = transform.GetChild(1);
        followers.SetParent(transform.parent);

        Style = _style;
        Size = _size;
        Softness = _softness;
        Opacity = _opacity;
    }

    public void StyleUpdate()
    {
        pointerBrushObjects.SetActive(PointerStyleIsActive);
        touchBrushObjects.SetActive(TouchStyleIsActive);

        touchStyleObjects[0].SetActive(Style == 0);
        touchStyleObjects[1].SetActive(Style == 1);
        touchStyleObjects[2].SetActive(Style == 2);
        touchStyleObjects[3].SetActive(Style == 4);

        touchStyleBlendObjects[0].SetActive(Style == 0);
        touchStyleBlendObjects[1].SetActive(Style == 1);
        touchStyleBlendObjects[2].SetActive(Style == 2);
        touchStyleBlendObjects[3].SetActive(Style == 4);

        styleHeadModels[0].SetActive(Style == 0 || Style == 3);
        styleHeadModels[1].SetActive(Style == 1);
        styleHeadModels[2].SetActive(Style == 2);
        styleHeadModels[3].SetActive(Style == 4 || Style == 5);

        for (int i = 0; i < pointerPaintingObjects.Length; i++)
        {
            pointerPaintingObjects[i].SetActive(Style == 3);
            pointerErasingObjects[i].SetActive(Style == 5);
        }

        touchColorPicker.farClipPlane = Style == 1 ? 0.125f : 0.1f;

        if (PointerStyleIsActive)
        {
            for (int i = 0; i < pointerProjectorCount; i++)
            {
                pointerPaintingObjects[i].SetActive(false);
                pointerErasingObjects[i].SetActive(false);
            }
        }

        pointerGuide.SetActive(false);
        pointerColorPickerGuide.SetActive(false);
    }

    public void SizeUpdate()
    {
        for (int i = 0; i < pointerProjectorCount; i++)
        {
            projectorAnimators[i].SetFloat("Size", Size);
        }

        for (int i = 0; i < touchProjectorAnimators.Length; i++)
        {
            touchProjectorAnimators[i].SetFloat("Size", Size);
        }

        pointerGuide.transform.localScale = Vector3.one * (Mathf.Clamp(Size * 1.5f, 0.01f, 1.5f) + 0.01f);
        step = Mathf.Lerp(0.00033f, 0.00066f, Size * 10.0f);

        var xz = Mathf.Lerp(0.0006f, 0.06f, Size);
        var newScale = new Vector3(xz, 1.0f, xz);
        brushHeadModels.localScale = newScale;
    }

    public void SoftnessUpdate()
    {
        for (int i = 0; i < brushOpacityMaterials.Length; i++)
        {
            brushOpacityMaterials[i].SetFloat("_Softness", Softness * 5.0f);
        }
    }

    public void OpacityUpdate()
    {
        var adjustedOpacity = Opacity * Opacity * Opacity;
        for (int i = 0; i < brushOpacityMaterials.Length; i++)
        {
            brushOpacityMaterials[i].SetFloat("_Alpha", adjustedOpacity);
        }
    }

    public override void OnPickup()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Event_UseUp));
    }

    public override void OnPickupUseDown()
    {
        if (uiPointerActive)
        {
            var ray = new Ray(raycastStart.position, raycastStart.forward);
            var hit = new RaycastHit();
            var clickHit = true;
            if (colorSliderCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                usingColorSlider = true;
                var value = Mathf.Clamp(Mathf.RoundToInt(hit.textureCoord.x * 100.0f), 0, 100);
                panel.Button_ColorSlider(value);
            }
            else if (sizeRangeCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                usingSizeRange = true;
                var x = hit.textureCoord.x * 1.01f - 0.005f;
                var value = Mathf.Clamp(Mathf.RoundToInt(x * 100.0f), 1, 100);
                panel.PlayRangeBlipAnimation(0, value);
                Size = value / 100.0f;
            }
            else if (softnessRangeCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                usingSoftnessRange = true;
                var x = hit.textureCoord.x * 1.01f - 0.005f;
                var value = Mathf.Clamp(Mathf.RoundToInt(x * 100.0f), 0, 100);
                panel.PlayRangeBlipAnimation(1, value);
                Softness = value / 100.0f;
            }
            else if (opacityRangeCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                usingOpacityRange = true;
                var x = hit.textureCoord.x * 1.01f - 0.005f;
                var value = Mathf.Clamp(Mathf.RoundToInt(x * 100.0f), 1, 100);
                panel.PlayRangeBlipAnimation(2, value);
                Opacity = value / 100.0f;
            }
            else if (ownerButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_Owner();
            }
            else if (pickupButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_Pickup();
            }
            else if (layerButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_Layers();
            }
            else if (syncButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_Sync();
            }
            else if (sizeMinusButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_SizeMinus();
            }
            else if (sizePlusButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_SizePlus();
            }
            else if (style0ButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                Style = 0;
                panel.PlayStyleBlipAnimation(Style);
            }
            else if (style1ButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                Style = 1;
                panel.PlayStyleBlipAnimation(Style);
            }
            else if (style2ButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                Style = 2;
                panel.PlayStyleBlipAnimation(Style);
            }
            else if (style3ButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                Style = 3;
                panel.PlayStyleBlipAnimation(Style);
            }
            else if (style4ButtonCollider.Raycast(ray, out hit, uiPointerRayRange) && panel.ShowLayerUI)
            {
                Style = 4;
                panel.PlayStyleBlipAnimation(Style);
            }
            else if (style5ButtonCollider.Raycast(ray, out hit, uiPointerRayRange) && panel.ShowLayerUI)
            {
                Style = 5;
                panel.PlayStyleBlipAnimation(Style);
            }
            else if (colorModeButtonCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_ColorMode();
            }
            else if (layer0Collider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_Layer0();
            }
            else if (layer1Collider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_Layer1();
            }
            else if (layer2Collider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_Layer2();
            }
            else if (layerEye2Collider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_LayerEye2();
            }
            else if (layerEye1Collider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_LayerEye1();
            }
            else if (layerEye0Collider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_LayerEye0();
            }
            else if (layerSwapCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_LayerSwap();
            }
            else if (layerBlendLeftCollider.Raycast(ray, out hit, uiPointerRayRange) && panel.ActiveLayer != 0)
            {
                panel.Button_LayerBlendLeft();
            }
            else if (layerBlendRightCollider.Raycast(ray, out hit, uiPointerRayRange) && panel.ActiveLayer != 0)
            {
                panel.Button_LayerBlendRight();
            }
            else if (layerEraseCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_LayerEraseStartPress();
                usingEraseButton = true;
            }
            else if (layerCopyCameraCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_LayerCopyCameraStartPress();
                usingCopyCameraButton = true;
            }
            else if (layerMergeCollider.Raycast(ray, out hit, uiPointerRayRange) && panel.ActiveLayer == 0)
            {
                panel.Button_LayerMergeStartPress();
                usingMergeButton = true;
            }
            else if (layerOpacitySliderCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                usingLayerOpacitySlider = true;
                var x = hit.textureCoord.x * 1.02f - 0.01f;
                var value = Mathf.Clamp(Mathf.RoundToInt(x * 100.0f), 0, 100);
                panel.Button_LayerOpacitySlider(value);
            }
            else
            {
                clickHit = false;
            }

            pointerAnim.SetTrigger(clickHit ? "Hit" : "Miss");
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Event_UseDown));
        }
    }

    public override void OnPickupUseUp()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Event_UseUp));
        usingColorSlider = false;
        usingLayerOpacitySlider = false;
        usingSizeRange = false;
        usingSoftnessRange = false;
        usingOpacityRange = false;
        usingEraseButton = false;
        usingMergeButton = false;
        usingCopyCameraButton = false;
    }

    public override void OnDrop()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Event_UseUp));
    }

    public void Event_UseDown()
    {
        useDown = true;
        firstFrameUseDown = true;

        touchPaintSet.SetActive(false);
        touchBlendSet.SetActive(true);

        if (TouchStyleIsActive)
        {
            for (int i = 0; i < touchProjectorAnimators.Length; i++)
            {
                touchProjectorAnimators[i].SetFloat("Size", Size);
            }
            var blendCameraSize = Mathf.Lerp(0.0006f, 0.06f, Size);
            if (Style == 2)
            {
                blendCameraSize *= 1.35f;
            }
            touchColorPicker.orthographicSize = blendCameraSize;
        }
    }

    public void Event_UseUp()
    {
        useDown = false;
        firstFrameUseDown = false;

        pointerColorPickerActive = false;
        pointerColorPickerCamera.SetActive(false);

        touchPaintSet.SetActive(true);
        touchBlendSet.SetActive(false);

        if (TouchStyleIsActive)
        {
            for (int i = 0; i < touchProjectorAnimators.Length; i++)
            {
                touchProjectorAnimators[i].SetFloat("Size", Size);
            }
        }

        if (PointerStyleIsActive)
        {
            for (int i = 0; i < pointerProjectorCount; i++)
            {
                pointerPaintingObjects[i].SetActive(false);
                pointerErasingObjects[i].SetActive(false);
            }
        }
    }

    private bool CanPaint(float currentStep)
    {
        var enoughAccumulated = distanceAccumulator >= currentStep;
        var enoughDisplacement = Vector3.Distance(lastPaintPosition, lastHitPosition) >= currentStep;
        return enoughAccumulated || enoughDisplacement || firstFrameUseDown;
    }

    public void Update()
    {
        // Motion smoothing
        var smoothingFactor = uiPointerActive ? 0.7f : 0.4f;
        followers.position = Vector3.Lerp(followers.position, transform.position, smoothingFactor);
        followers.rotation = Quaternion.Slerp(followers.rotation, transform.rotation, smoothingFactor);

        // Initialize brush color
        if (initTimer >= 0.0f)
        {
            initTimer += Time.deltaTime;
            if (initTimer > 2.0f)
            {
                initTimer = -1.0f;
                colorInitializer.SetActive(false);
            }
        }

        // UI pointer
        var ray = new Ray(raycastStart.position, raycastStart.forward);
        var hit = new RaycastHit();
        var useHoldLockout = !uiPointerActive && useDown;
        if (!useHoldLockout && pointerCollider.Raycast(ray, out hit, uiPointerRayRange) || (panel.ShowLayerUI && layerUICollider.Raycast(ray, out hit, uiPointerRayRange)))
        {
            uiPointerActive = true;
            pointerSet.SetActive(true);
            paintingSet.SetActive(false);
            pointerLine.localScale = new Vector3(1, 1, hit.distance);
            pointerDot.localPosition = new Vector3(0, 0, hit.distance);
        }
        else
        {
            if (uiPointerActive)
            {
                uiPointerActive = false;
                if (activatedBlips)
                {
                    panel.HideOtherBlip(0);
                    panel.HideOtherBlip(1);
                    panel.HideOtherBlip(2);
                    panel.HideStyleBlips();
                }
            }
            activatedBlips = false;
            pointerSet.SetActive(false);
            paintingSet.SetActive(true);
        }

        // Current brush property indicators
        if (uiPointerActive && LocalPlayerOwnsThis)
        {
            activatedBlips = true;
            panel.ShowOtherBlip(0, Mathf.RoundToInt(Size * 100.0f));
            panel.ShowOtherBlip(1, Mathf.RoundToInt(Softness * 100.0f));
            panel.ShowOtherBlip(2, Mathf.RoundToInt(Opacity * 100.0f));
            panel.ShowStyleBlip(Style);
        }

        // Color slider drag
        if (usingColorSlider)
        {
            if (colorSliderCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                var value = Mathf.Clamp(Mathf.RoundToInt(hit.textureCoord.x * 100.0f), 0, 100);
                panel.Button_ColorSliderDrag(value);
            }
            else
            {
                usingColorSlider = false;
            }
        }

        // Layer opacity slider drag
        if (usingLayerOpacitySlider)
        {
            if (layerOpacitySliderCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                var x = hit.textureCoord.x * 1.02f - 0.01f;
                var value = Mathf.Clamp(Mathf.RoundToInt(x * 100.0f), 0, 100);
                panel.Button_LayerOpacitySliderDrag(value);
            }
            else
            {
                usingLayerOpacitySlider = false;
            }
        }

        // Size range drag
        if (usingSizeRange)
        {
            if (sizeRangeCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                var x = hit.textureCoord.x * 1.01f - 0.005f;
                var value = Mathf.Clamp(Mathf.RoundToInt(x * 100.0f), 1, 100);
                panel.PlayRangeBlipAnimation(0, value);
                Size = value / 100.0f;
            }
            else
            {
                usingSizeRange = false;
            }
        }

        // Softness range drag
        if (usingSoftnessRange)
        {
            if (softnessRangeCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                usingSoftnessRange = true;
                var x = hit.textureCoord.x * 1.01f - 0.005f;
                var value = Mathf.Clamp(Mathf.RoundToInt(x * 100.0f), 0, 100);
                panel.PlayRangeBlipAnimation(1, value);
                Softness = value / 100.0f;
            }
            else
            {
                usingSoftnessRange = false;
            }
        }

        // Opacity range drag
        if (usingOpacityRange)
        {
            if (opacityRangeCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                usingOpacityRange = true;
                var x = hit.textureCoord.x * 1.01f - 0.005f;
                var value = Mathf.Clamp(Mathf.RoundToInt(x * 100.0f), 1, 100);
                panel.PlayRangeBlipAnimation(2, value);
                Opacity = value / 100.0f;
            }
            else
            {
                usingOpacityRange = false;
            }
        }

        // Erase confirm
        if (usingEraseButton)
        {
            if (layerEraseCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_LayerEraseHold();
            }
            else
            {
                usingEraseButton = false;
            }
        }

        // Merge confirm
        if (usingMergeButton)
        {
            if (layerMergeCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_LayerMergeHold();
            }
            else
            {
                usingMergeButton = false;
            }
        }

        // Copy camera confirm
        if (usingCopyCameraButton)
        {
            if (layerCopyCameraCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                panel.Button_LayerCopyCameraHold();
            }
            else
            {
                usingCopyCameraButton = false;
            }
        }

        // Pointer brush
        if (PointerStyleIsActive)
        {
            // Paint on canvas
            if (canvasCollider.Raycast(ray, out hit, pointerBrushRayRange))
            {
                pointerGuide.SetActive(!useDown);
                pointerGuide.transform.position = hit.point + (hit.normal * 0.0011f);
                pointerGuide.transform.forward = -hit.normal;

                if (lastRayMissed)
                {
                    lastRayMissed = false;
                    lastPaintPosition = hit.point;
                    lastHitPosition = hit.point;
                }

                distanceAccumulator += Vector3.Distance(lastHitPosition, hit.point);
                distanceAccumulator = Mathf.Min(distanceAccumulator, step);
                lastHitPosition = hit.point;

                var gap = Vector3.Distance(lastPaintPosition, lastHitPosition) / (pointerProjectorCount - 1);
                var currentStep = Mathf.Max(step, gap);

                for (int i = 0; i < pointerProjectorCount && CanPaint(currentStep); i++)
                {
                    distanceAccumulator -= currentStep;
                    distanceAccumulator = Mathf.Max(distanceAccumulator, 0.0f);
                    lastPaintPosition = Vector3.MoveTowards(lastPaintPosition, hit.point, currentStep);
                    projectorTransforms[i].position = lastPaintPosition + (hit.normal * -0.03f);
                    projectorTransforms[i].forward = hit.normal;
                    projectorsOn = i + 1;
                    firstFrameUseDown = false;
                }
            }
            else
            {
                pointerGuide.SetActive(false);
                lastRayMissed = true;

                // Enable color picker
                if (!uiPointerActive && firstFrameUseDown)
                {
                    pointerColorPickerActive = true;
                    pointerColorPickerCamera.SetActive(useDown);
                }
            }

            // Color picker guide
            if (paletteCollider.Raycast(ray, out hit, uiPointerRayRange))
            {
                pointerColorPickerGuide.SetActive(true);
                pointerColorPickerGuide.transform.position = hit.point + (hit.normal * 0.001f);
                pointerColorPickerGuide.transform.forward = -hit.normal;
            }
            else
            {
                pointerColorPickerGuide.SetActive(false);
            }

            // Enable pointer brush projectors
            for (int i = 0; i < pointerProjectorCount; i++)
            {
                var show = !pointerColorPickerActive && useDown && i < projectorsOn;
                if (Style == 3)
                {
                    if (show && !pointerPaintingObjects[i].activeSelf)
                    {
                        pointerPaintingObjects[i].SetActive(true);
                        projectorAnimators[i].SetFloat("Size", Size);
                    }
                    else if (!show && pointerPaintingObjects[i].activeSelf)
                    {
                        pointerPaintingObjects[i].SetActive(false);
                    }
                }
                else
                {
                    if (show && !pointerErasingObjects[i].activeSelf)
                    {
                        pointerErasingObjects[i].SetActive(true);
                        projectorAnimators[i].SetFloat("Size", Size);
                    }
                    else if (!show && pointerErasingObjects[i].activeSelf)
                    {
                        pointerErasingObjects[i].SetActive(false);
                    }
                }
            }
            projectorsOn = 0;
            firstFrameUseDown = false;
        }
    }
}
