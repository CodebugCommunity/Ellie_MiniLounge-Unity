
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class GaleToggle : UdonSharpBehaviour
{
    [SerializeField] private UdonInteractEvent galeHandler;
    bool isMuted = false;

    [SerializeField] private GameObject onState;
    [SerializeField] private GameObject offState;
    
    void Start()
    {
        
    }
    
    public override void Interact()
    {
        isMuted = !isMuted;
        if (isMuted)
        {
            onState.SetActive(false);
            offState.SetActive(true);
        }
        else
        {
            onState.SetActive(true);
            offState.SetActive(false);
        }
        galeHandler.SetMuteState(isMuted);
    }
}
