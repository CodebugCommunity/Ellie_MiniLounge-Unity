
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DaynightSwitcher : UdonSharpBehaviour
{
    [SerializeField]GameObject day;
    [SerializeField]GameObject night;
    
    [SerializeField]Material daySkybox;
    [SerializeField]Material nightSkybox;
    
    void Start()
    {
        SetTimeOfDay();
    }
    bool isDay = true;
    
    public override void Interact()
    {
        isDay = !isDay;
        
        SetTimeOfDay();
        
    }

    void SetTimeOfDay()
    {
        if (isDay)
        {
            day.SetActive(true);
            night.SetActive(false);
            RenderSettings.skybox = daySkybox;
        }else
        {
            day.SetActive(false);
            night.SetActive(true);
            RenderSettings.skybox = nightSkybox;
        }
    }
}
