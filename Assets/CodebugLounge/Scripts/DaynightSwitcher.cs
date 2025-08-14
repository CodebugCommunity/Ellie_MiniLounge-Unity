
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DaynightSwitcher : UdonSharpBehaviour
{
    
    [SerializeField] GameObject day;
    [SerializeField]GameObject night;
    
    [SerializeField]Material daySkybox;
    [SerializeField]Material nightSkybox;
    
    public bool state = false;
    Transform switchTransform;
    
    public override void Interact()
    {
        state = !state;
        UpdateVisuals();
       
        
        isDay = !isDay;
        
        SetTimeOfDay();
    }
    
    void UpdateVisuals()
    {
        switchTransform.localRotation = Quaternion.Euler(state ? -90 : 90, 0,0);
    }
    
    void Start()
    {
        switchTransform = transform.GetChild(1);
        SetTimeOfDay();
    }
    
    bool isDay = false;

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
