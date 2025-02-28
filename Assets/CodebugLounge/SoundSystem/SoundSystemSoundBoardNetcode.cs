
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
namespace SoundSystem {
[DefaultExecutionOrder(-1)]
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SoundSystemSoundBoardNetcode : UdonSharpBehaviour
{
    [UdonSynced, FieldChangeCallback("micVolume")] float _micVolume;
    [UdonSynced, FieldChangeCallback("muted")] public bool _muted;
    [UdonSynced, FieldChangeCallback("locked")] public bool _locked;
    [SerializeField] int channel;
    [SerializeField] SoundSystemSoundBoard micController;
    [SerializeField] Image muteIndicator;
    [SerializeField] Image lockIndicator;

    public float micVolume {
        get => _micVolume;

        set {
            micController.SetMicLevel(muted?-1:value, channel - 1);
            GetComponentInChildren<Slider>().value = value;
            _micVolume = value;
        }
    }
    public bool muted {
        get => _muted;

        set {
            _muted = value;
            micVolume = micVolume; //set the micController's value to level 0. This is handeled in the setter
            muteIndicator.color = value?Color.green:new Color(0, (float)0.5, 0); //make it a lighter green when muted
        }
    }
    public bool locked {
        get => _locked;

        set {
            _locked = value;
            lockIndicator.color = value?Color.red:new Color((float)0.5, 0, 0);
        }
    }
    void Start()
    {
        Debug.Log("Starting B");
        _micVolume = 10;
        muted = false;
        locked = false;
    }

    //Run by the slider itself when it is changed.
    public void OnChangeVolumeLevel() {
        if (!locked || (Networking.LocalPlayer.displayName == micController.microphoneOwners[channel - 1])) {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            micVolume = GetComponentInChildren<Slider>().value;
            RequestSerialization();
        }
        else {
            GetComponentInChildren<Slider>().value = micVolume;
        }
    }

    public void OnToggleMute() {
        if (!locked || (Networking.LocalPlayer.displayName == micController.microphoneOwners[channel - 1])) {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            muted = !muted;
            RequestSerialization();
        }
    }

    public void OnToggleLock() {
        if (Networking.LocalPlayer.displayName == micController.microphoneOwners[channel - 1]) {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            locked = !locked;
            RequestSerialization();
        }
    }
}
}