
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RugSwitch : UdonSharpBehaviour
{
    public bool state = false;
    Transform switchTransform;
    
    public MeshRenderer[] ObjectsToToggle;
    
    public override void Interact()
    {
        state = !state;
        UpdateVisuals();
        foreach (var o in ObjectsToToggle)
        {
            o.sharedMaterial.SetFloat("_LayerCount", state ? 3 : 1);
        }
    }
    
    void UpdateVisuals()
    {
        switchTransform.localRotation = Quaternion.Euler(state ? -90 : 90, 0,0);
    }
    
    void Start()
    {
        switchTransform = transform.GetChild(1);
        UpdateVisuals();
    }
}
