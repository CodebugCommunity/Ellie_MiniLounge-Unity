
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]  
public class NotificationHud : UdonSharpBehaviour
{
     [Header("Notification Settings")]
    public bool ShowJoinNotifications = true;
    public bool ShowLeaveNotifications = true;
    [Space(30)]
    [Header("Icon Settings")]
    public Sprite AttentionSprite;
    
    [Header("Audio Settings")]
    public AudioClip JoinAudio;
    
    [Space(30)]

    //Don't touch these.
    public Text HUDJoinMessageText;
    public Text HUDInfoText;
    public Animator LocalAnimator;
    public AudioSource NotificationJoinAudio;

    public string NotificationText;
    public string InfoText;
    
    public Image MainImage;
    public Image Background;
    public VRCPlayerApi.TrackingDataType trackingTarget;

    VRCPlayerApi playerApi;
    bool isInEditor;
    
    public void CreateNotifitcation(string ownerName)
    {
        if (ShowJoinNotifications)
        {
            SetJoin();
            HUDJoinMessageText.text = ownerName + " " + NotificationText;
            LocalAnimator.SetTrigger("PlayJoinMessage");
            NotificationJoinAudio.Play();
        }
    }

    private void LateUpdate()
    {
        if (isInEditor)
            return;

        VRCPlayerApi.TrackingData trackingData = playerApi.GetTrackingData(trackingTarget);
        transform.SetPositionAndRotation(trackingData.position, trackingData.rotation);
    }

    public void Start()
    {
        //Check for EditorMode
        playerApi = Networking.LocalPlayer;
        isInEditor = playerApi == null;

        //Set up Audio
        NotificationJoinAudio.clip = JoinAudio;
        
        
        
    }

    public void SetLeave()
    {
        MainImage.sprite = AttentionSprite;
        Background.sprite = AttentionSprite;
        HUDInfoText.text = "Player Left";
    }

    public void SetJoin()
    {
        MainImage.sprite = AttentionSprite;
        Background.sprite = AttentionSprite;
        HUDInfoText.text = "";
    }

    
}
