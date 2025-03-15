
using UdonSharp;
using UnityEngine;
using UnityEngine.Events;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class UdonInteractEvent : UdonSharpBehaviour
{
    [SerializeField] NotificationHud notificationHud;
    
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
    
    public void GaleKiss()
    {
        if(Networking.GetOwner(gameObject) != Networking.LocalPlayer)
        {
            Debug.Log("GaleKiss");
            notificationHud.CreateNotifitcation(Networking.GetOwner(gameObject).displayName);
        }
        
    }
    
    void Start()
    {
        
    }
}
