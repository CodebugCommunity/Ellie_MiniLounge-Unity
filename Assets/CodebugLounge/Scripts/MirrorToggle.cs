
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MirrorToggle : UdonSharpBehaviour
{
    public GameObject mirror;
    
    void Start()
    {
        InteractionText = "Toggle Mirror";
    }
    
    public override void Interact()
    {
        mirror.SetActive(!mirror.activeSelf);
    }

}
