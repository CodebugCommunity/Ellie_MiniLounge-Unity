
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using Cysharp.Threading.Tasks.Triggers;
using System;
using Microsoft.Win32;
using Yamadev.YamaStream;
namespace SoundSystem {
[DefaultExecutionOrder(-2)]
public class SoundSystemSoundBoard : UdonSharpBehaviour
{
    [SerializeField] UdonBehaviour[] microphones;
    private string[] _microphoneOwners;
    public bool localPlayerInCollider;

    [SerializeField] TMP_Text[] namePlates;
    [SerializeField] Controller yamaPlayerController;

    private float[] micLevels;
    public void Start() {
        Debug.Log("Starting A");

        micLevels = new float[microphones.Length];
        for (int i = 0; i < micLevels.Length; i++) {
            micLevels[i] = 10; //default value
        }
        _microphoneOwners = new string[microphones.Length];
        for (int i = 0; i < microphoneOwners.Length; i++) {
            _microphoneOwners[i] = ""; //value used for no owner
        }
    }
    public string[] microphoneOwners
    {
        get => _microphoneOwners;

        set {
            Debug.Log("RECIEVED");
            Debug.Log("AHHHH " + microphoneOwners.Length);
            //Check all pairs of array elements. If there is a difference between the new and current string, call function
            for (int i = 0; i < microphoneOwners.Length; i++) {
                namePlates[i].text = "Held By\n" + value[i];
                if (microphoneOwners[i] != value[i] & localPlayerInCollider == true) {
                    OnChangeMicrophoneOwner(i, microphoneOwners[i], value[i]);
                }
            }
            _microphoneOwners = value;
            Debug.Log(microphoneOwners[0]);
        }
    }

    public void OnChangeMicrophoneOwner(int microphoneIndex, string oldName, string newName) {
        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);
        Debug.Log("G" + microphoneIndex.ToString()); //This doesn't get logged
        //Remove Old Player's Microphone
        Debug.Log(newName); Debug.Log(oldName);   
        if (oldName != "") {
            for (int i = 0; i < players.Length; i++) {
                if (players[i] != null && players[i].displayName.Equals(oldName)) {
                    ActivateMic(players[i], false, microphoneIndex);
                }
            }
        }
        //Give New Player Microphone
        if (newName != "") {
            for (int i = 0; i < players.Length; i++) {
                if (players[i] != null && players[i].displayName.Equals(newName)) {
                    ActivateMic(players[i], true, microphoneIndex);
                }
            }
        }
    }
    public void ActivateMic(VRCPlayerApi player, bool activatingMic, int microphoneIndex) {
        if (activatingMic) {
            Debug.Log("Activating Mic " + player.displayName);
            Debug.Log("A" + microphoneIndex.ToString());
            Debug.Log("B" + micLevels.Length.ToString());
            Debug.Log(microphoneOwners.Length);
            if (micLevels[microphoneIndex] == -1) {
                player.SetVoiceDistanceNear(0);
                player.SetVoiceDistanceFar(0);
            }
            else {
                player.SetVoiceDistanceNear(1000);
                player.SetVoiceDistanceFar(1050);
                player.SetVoiceGain(micLevels[microphoneIndex]);
                player.SetVoiceLowpass(false);
            }
        }
        else {
            Debug.Log("Deactivating Mic " + player.displayName);
            player.SetVoiceDistanceNear(0);
            player.SetVoiceDistanceFar(25);
            player.SetVoiceGain(15);
            player.SetVoiceLowpass(true);
        }
        Debug.Log(player.GetVoiceGain());
    }

    public void PlayerEntersTrigger() {
        localPlayerInCollider = true;

        //Tell YamaPlayer To Unmute
        yamaPlayerController.UpdateAudio();

        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        //Iterate through all mic owners and change their voicerange
        for (int j = 0; j < microphoneOwners.Length; j++) {
            string player = microphoneOwners[j];
            if (player != "") {
                for (int i = 0; i < players.Length; i++) {
                    if (players[i] != null && players[i].displayName.Equals(player)) {
                        ActivateMic(players[i], true, j);
                    }
                }
            }
        }
    }

    public void PlayerExitsTrigger() {
        localPlayerInCollider = false;

        //Tell YamaPlayer To Mute
        yamaPlayerController.UpdateAudio();
        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        //Iterate through all mic owners and change their voicerange
        for (int j = 0; j < microphoneOwners.Length; j++) {
            string player = microphoneOwners[j];
            if (player != "") {
                for (int i = 0; i < players.Length; i++) {
                    if (players[i] != null && players[i].displayName.Equals(player)) {
                        ActivateMic(players[i], false, j);
                    }
                }
            }
        }
    }

    public void SetMicLevel(float level, int micIndex) {
        micLevels[micIndex] = level;
        if (microphoneOwners[micIndex] != "" && localPlayerInCollider) {
            string player = microphoneOwners[micIndex];
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            for (int i = 0; i < players.Length; i++) {
                if (players[i] != null && players[i].displayName.Equals(player)) {
                    Debug.Log("C " + i.ToString());
                    Debug.Log("D " + micLevels.Length.ToString());
                    Debug.Log("E " + micIndex.ToString());
                    Debug.Log(micLevels[micIndex]);
                    ActivateMic(players[i], true, micIndex);
                }
            }
        } 
    }
}
}