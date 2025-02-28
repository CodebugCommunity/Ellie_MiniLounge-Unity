
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace SoundSystem {
public class SoundSystemMicAudioRange : UdonSharpBehaviour
{
    [SerializeField] SoundSystemSoundBoard micController;
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal) {
            micController.PlayerEntersTrigger();
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.isLocal) {
            micController.PlayerExitsTrigger();
        }
    }
}
}