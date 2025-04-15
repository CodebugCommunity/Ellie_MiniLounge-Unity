
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class PaintingHandler : UdonSharpBehaviour
{
    public GameObject[] gameObjectsToDisable;
    public GameObject[] gameObjectsToEnable;
    
    public override void Interact()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetState));
        
        Debug.Log("Interact");
    }
    
    public void SetState()
    {
        foreach (var o in gameObjectsToEnable)
        {
            o.SetActive(true);    
        }
        
        foreach (var o in gameObjectsToDisable)
        {
            o.SetActive(false);    
        }
        
    }
    void Start()
    {
        
    }
}
