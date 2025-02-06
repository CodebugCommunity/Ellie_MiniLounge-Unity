
using System.Collections;
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

public class CodebugFeeder : UdonSharpBehaviour
{
    public NavMeshAgent[] codebugs;

    public Transform randomCenter;
    
    float timer = 0;
    
    void Start()
    {
        InteractionText = "Feed Codebugs";
        DisableInteractive = false;
    }
    
    public override void Interact()
    {
        foreach (NavMeshAgent codebug in codebugs)
        {
            codebug.SetDestination(transform.position);
            timer = 8;
        }
    }
    
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer<0)
        {
            foreach (NavMeshAgent codebug in codebugs)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 13;
                codebugs[Random.Range(0,codebugs.Length)].SetDestination(randomCenter.position + randomDirection);
            }
            
            timer = 1;
        }
    }
}
