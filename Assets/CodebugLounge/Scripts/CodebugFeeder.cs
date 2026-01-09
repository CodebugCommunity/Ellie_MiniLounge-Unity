
using System;
using System.Collections;
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;
using Random = UnityEngine.Random;
using VRC.Udon.Common.Interfaces;


public class CodebugFeeder : UdonSharpBehaviour
{

    [SerializeField] ParticleSystem pelletParticles;

    [SerializeField] NavMeshAgent[] codebugs;

    [SerializeField] Transform[] randomCenters;
    
    [SerializeField]CodebugState[] codebugStates  = new CodebugState[20];
    Vector4[] positions = new Vector4[20];
    [SerializeField] float targetUpdateFreq = 1;
    [SerializeField] float transitUpdateFreq = 5;
    
    [SerializeField] private Renderer[] grassRenderers;
    private Material grassMaterial;
    
    float timer = 0;
    float timerTransit = 0;
    
    private CodebugState GetClosestState(Vector3 position)
    {
        float minDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < randomCenters.Length; i++)
        {
            float distance = Vector3.Distance(position, randomCenters[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return (CodebugState)closestIndex;
    }


    void Start()
    {
        InteractionText = "Feed Codebugs";
        DisableInteractive = false;
        
        if(Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            for (int i = 0; i < codebugs.Length; i++)
            {
                codebugStates[i] = GetClosestState(codebugs[i].gameObject.transform.position);
            }
        }
        

        grassMaterial = grassRenderers[0].material;
        
    }

    public void CallCodebugs()
    {
        foreach (NavMeshAgent codebug in codebugs)
        {
            codebug.SetDestination(transform.position);
        }
        
        timer = 8;
    }

    public void PlayParticles()
    {
        pelletParticles.Play();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi newOwner)
    {
        if(Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            for (int i = 0; i < codebugs.Length; i++)
            {
                codebugStates[i] = GetClosestState(codebugs[i].gameObject.transform.position);
            }
        }
    }

    public override void Interact()
    {
        
        SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(CallCodebugs));
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayParticles));
    }
    
    private void Update()
    {


        for (int i = 0; i < codebugs.Length; i++)
        {
            positions[i] = codebugs[i].gameObject.transform.position;
        }

        foreach (var renderer in grassRenderers)
        {
            renderer.material.SetVectorArray("_PlayerPositions", positions);
        }


        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            foreach (NavMeshAgent codebug in codebugs)
            {
                codebug.enabled = false;
            }

            return;
        }
        else
        {
            foreach (NavMeshAgent codebug in codebugs)
            {
                codebug.enabled = true;
            }
        }


       
        
        
        
        timer -= Time.deltaTime;
        
        if (timer<0)
        {
            for (int i = 0; i < codebugs.Length; i++)
            {
                Transform randomCenter = randomCenters[(int)codebugStates[i]];
                Vector3 randomDirection = Random.insideUnitSphere * (randomCenter.localScale.x * 0.5f);
                
                codebugs[i].SetDestination(randomCenter.position + randomDirection);
            }
            
            timer = targetUpdateFreq;
        }

        timerTransit -= Time.deltaTime;
        if (timerTransit < 0)
        {
            codebugStates[Random.Range(0, codebugs.Length)] = (CodebugState)Random.Range(0, 3);
            
            timerTransit = transitUpdateFreq;
        }
    }
}

enum CodebugState
{
    Main,
    Wall,
    Center
}
