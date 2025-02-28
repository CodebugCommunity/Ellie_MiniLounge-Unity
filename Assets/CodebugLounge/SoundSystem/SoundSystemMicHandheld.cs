
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
namespace SoundSystem {
public class SoundSystemMicHandheld : UdonSharpBehaviour
{
    [SerializeField] int channel;
    [SerializeField] SoundSystemMicNetworkDummy networkDummy;
    
    public void Start() {
        Debug.Log("Starting C");
        networkDummy.SetMicChannel(channel);
    }
    public override void OnPickup() {
        //Sends New Owner To Controller
        VRCPlayerApi temp = ((VRCPickup)GetComponent(typeof(VRCPickup))).currentPlayer;
        if (temp.isLocal) {
            if (temp == null) {
                networkDummy.NewMicrophoneOwner("");
            }
            else {
                networkDummy.NewMicrophoneOwner(temp.displayName);
            }
    }
    }
    public override void OnDrop() {
        networkDummy.NewMicrophoneOwner("");
    }
}
}