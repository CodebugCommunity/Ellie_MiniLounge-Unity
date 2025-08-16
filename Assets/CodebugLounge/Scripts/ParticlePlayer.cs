
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
public class ParticlePlayer : UdonSharpBehaviour
{

    [SerializeField]ParticleSystem particleSystem;
    void Start()
    {
        
    }
    
    public void Fireworks()
    {
        if (!particleSystem.isPlaying)
            particleSystem.Play();
    }

    public override void Interact()
    {

        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Fireworks));

    }
}
