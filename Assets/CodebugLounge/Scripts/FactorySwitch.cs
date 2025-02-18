
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FactorySwitch : UdonSharpBehaviour
{
    
    public bool state = false;
    Transform switchTransform;
    
    public GameObject[] ObjectsToToggle;
    
    public override void Interact()
    {
        state = !state;
        UpdateVisuals();
        foreach (GameObject o in ObjectsToToggle)
        {
            o.SetActive(state);
        }
    }
    
    void UpdateVisuals()
    {
        switchTransform.localRotation = Quaternion.Euler(state ? -90 : 90, 0,0);
    }
    
    void Start()
    {
        switchTransform = transform.GetChild(1);
    }
}
