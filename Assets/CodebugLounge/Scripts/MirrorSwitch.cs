
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MirrorSwitch : UdonSharpBehaviour
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
            if (o != null)
            {
                o.SetActive(state);
            }
            
        }
    }
    
    void UpdateVisuals()
    {
        switchTransform.localRotation = Quaternion.Euler(state ? -45 : 45, 0,0);
    }
    
    void Start()
    {
        switchTransform = transform.GetChild(1);
        UpdateVisuals();
    }
}
