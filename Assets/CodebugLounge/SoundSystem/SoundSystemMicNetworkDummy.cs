
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace SoundSystem {
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SoundSystemMicNetworkDummy : UdonSharpBehaviour
{
    [SerializeField] private SoundSystemSoundBoard micController;
    private int micChannel;
    [UdonSynced, FieldChangeCallback("owner")] private string _owner;
    [SerializeField] SoundSystemMicPack micPack; //ONLY SET IF IT IS A MIC PACK

    public string owner {
        get => _owner;

        set {
            string[] temp = new string[micController.microphoneOwners.Length];
            Array.Copy(micController.microphoneOwners, temp, temp.Length);
            Debug.Log(temp[0]);
            Debug.Log(micChannel);
            temp[micChannel - 1] = value;
            micController.microphoneOwners = temp;
            _owner = value;
            if (micPack != null) {
                micPack.UpdateLabels(owner);
            }
        }
    }
    public void NewMicrophoneOwner(string microphoneOwner) {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        owner = microphoneOwner;
        RequestSerialization();
    }

    public void SetMicChannel(int channel) {
        micChannel = channel;
        Debug.Log(micChannel);
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (owner == player.displayName) {
            owner = ""; //DOES NOT SERIALIZE SO THAT IT DOESNT UPDATE NETCODE
            base.OnPlayerLeft(player);
        }
    }
}
}