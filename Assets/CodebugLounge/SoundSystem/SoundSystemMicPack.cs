
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace SoundSystem {
public class SoundSystemMicPack : UdonSharpBehaviour
{
    [SerializeField] int channel;
    [SerializeField] SoundSystemMicNetworkDummy networkDummy;
    [SerializeField] TMP_Text namePlate;
    [SerializeField] TMP_Text buttonLabel;
    
    public void Start() {
        networkDummy.SetMicChannel(channel);
    }
    private void TakeMic() {
        //Sends New Owner To Controller
        networkDummy.NewMicrophoneOwner(Networking.LocalPlayer.displayName);
    }
    private void ReturnMic() {
        networkDummy.NewMicrophoneOwner("");
    }
    public void OnPress() {
        if (networkDummy.owner == "") {
            TakeMic();
        }
        else if (networkDummy.owner == Networking.LocalPlayer.displayName) {
            ReturnMic();
        }
    }
    public void UpdateLabels(string newOwner) {
        if (newOwner != "") {
           namePlate.text = "In Use\n" + newOwner;
            buttonLabel.text = "Return";
        }
        else {
            namePlate.text = "Available";
            buttonLabel.text = "Take";
        }
    }
}
}