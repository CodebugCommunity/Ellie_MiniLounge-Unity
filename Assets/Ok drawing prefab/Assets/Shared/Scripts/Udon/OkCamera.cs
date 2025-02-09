
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using UdonSharp;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class OkCamera : UdonSharpBehaviour
{
    public GameObject cameraObject;
    public Slider fovSlider;
    public Camera screenshotCamera;
    private Transform followers;
    private float previewFovTimer = -1.0f;
    private bool LocalPlayerOwnsThis { get { return Networking.IsOwner(Networking.LocalPlayer, gameObject); } }
    [UdonSynced, FieldChangeCallback(nameof(FOV))] private float _fov = 0.35f; public float FOV { get { return _fov; } set { _fov = value; FOVUpdate(); } }

    public void Start()
    {
        if (followers == null)
        {
            followers = transform.GetChild(0);
            followers.SetParent(transform.parent);
        }
        cameraObject.SetActive(false);
        FOV = _fov;
    }

    public void SetVisible(bool canSee, bool hasPermission)
    {
        if (followers == null)
        {
            followers = transform.GetChild(0);
            followers.SetParent(transform.parent);
        }
        var vrcPickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        followers.gameObject.SetActive(canSee);

#if UNITY_ANDROID
        vrcPickup.pickupable = false;
#else
        vrcPickup.pickupable = canSee && hasPermission;
#endif
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (LocalPlayerOwnsThis)
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Event_UseUp));
        }
    }

    public override void OnPickup()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Event_UseUp));
    }

    public override void OnPickupUseDown()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Event_UseDown));
    }

    public override void OnPickupUseUp()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Event_UseUp));
    }

    public override void OnDrop()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Event_UseUp));
    }

    public void Event_UseDown()
    {
        cameraObject.SetActive(true);
    }

    public void Event_UseUp()
    {
        cameraObject.SetActive(false);
    }

    public void FOVUpdate()
    {
        screenshotCamera.fieldOfView = Mathf.Lerp(25, 110, FOV);
        fovSlider.value = FOV;
        previewFovTimer = 0.2f;
    }

    public void Update()
    {
        // Motion smoothing
        var smoothingFactor = 0.4f;
        followers.position = Vector3.Lerp(followers.position, transform.position, smoothingFactor);
        followers.rotation = Quaternion.Slerp(followers.rotation, transform.rotation, smoothingFactor);

        // Fov slider
        if (FOV != fovSlider.value)
        {
            if (LocalPlayerOwnsThis)
            {
                FOV = fovSlider.value;
            }
            else
            {
                fovSlider.value = FOV;
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
        }
        if (previewFovTimer >= 0.0f)
        {
            previewFovTimer -= Time.deltaTime;
            cameraObject.SetActive(true);
            if (previewFovTimer <= 0.0f)
            {
                previewFovTimer = -1.0f;
                cameraObject.SetActive(false);
            }
        }
    }
}
