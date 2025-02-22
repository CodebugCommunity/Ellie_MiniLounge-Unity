
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Teleportation : UdonSharpBehaviour
{
    
    [SerializeField] Transform teleportTarget;
    [SerializeField] GameObject[] objToEnable;
    [SerializeField] GameObject[] objToDisable;
    
    void Start()
    {
        
    }
    
    public override void Interact()
    {
        Networking.LocalPlayer.TeleportTo(teleportTarget.position, teleportTarget.rotation);

        foreach (var obj in objToEnable)
        {
            obj.SetActive(true);
        }
        
        foreach (var obj in objToDisable)
        {
            obj.SetActive(false);
        }
    }
}
