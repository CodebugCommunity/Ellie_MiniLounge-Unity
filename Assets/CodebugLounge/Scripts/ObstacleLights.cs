
using System;
using System.Collections;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ObstacleLights : UdonSharpBehaviour
{
    
    [SerializeField] private GameObject[] lights;
    [SerializeField] private float timeout;
    
    float timer = 0.5f;
    bool lightOn = false;
    
    void Start()
    {
        
    }
    
    

    private void Update()
    {
        timer -= Time.deltaTime;
        
        if (timer <= 0 && !lightOn)
        {
            timer = 0.2f;
            foreach (var light in lights)
            {
                light.SetActive(true);
            }
            lightOn = true;
        }
        
        if (timer <= 0 && lightOn)
        {
            timer = 2f;
            foreach (var light in lights)
            {
                light.SetActive(false);
            }
            lightOn = false;
        }
    }
}
