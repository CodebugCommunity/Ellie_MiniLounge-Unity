
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ParticlePlayer : UdonSharpBehaviour
{

    [SerializeField]ParticleSystem particleSystem;
    void Start()
    {
        
    }
    
    public override void Interact()
    {
        if(!particleSystem.isPlaying)
            particleSystem.Play();
    }
}
