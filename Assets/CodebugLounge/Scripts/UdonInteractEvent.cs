
using UdonSharp;
using UnityEngine;
using UnityEngine.Events;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Enums;
using VRC.Udon.Common.Interfaces;

public class UdonInteractEvent : UdonSharpBehaviour
{
    [SerializeField] NotificationHud notificationHud;
    [SerializeField] AudioSource _audioSource;
    bool isMuted = false;
    bool isOnCooldown = false;
    public void SetMuteState(bool state)
    {
        isMuted = state;
    }
    public override void OnPickupUseDown()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(GaleKiss));
        Debug.Log("OnPickupUseDownGale");
    }
    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(GaleKiss));
        Debug.Log("OnInteractGale");
        
    }
    
    public void EndCooldown()
    {
        isOnCooldown = false;
    }
    
    public void GaleKiss()
    {
        Debug.Log("GaleKissReceived. IsMuted:" + isMuted + " IsOnCooldown:" + isOnCooldown);
        if (!isMuted && !isOnCooldown)
        {
            _audioSource.Play();
            
            if(Networking.GetOwner(gameObject) != Networking.LocalPlayer)
            {
                notificationHud.CreateNotifitcation(Networking.GetOwner(gameObject).displayName);
                Debug.Log("GaleKissNotificationShown");
            }
        }
        
        
        
        
        if (!isOnCooldown)
        {
            isOnCooldown = true;
            SendCustomEventDelayedSeconds(nameof(EndCooldown), 3);    
        }
        
    }
    
    void Start()
    {
        
    }
}
