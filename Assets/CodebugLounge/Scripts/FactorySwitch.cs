
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRCStation = VRC.SDK3.Components.VRCStation;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(FactorySwitch))]
public class EventButtonEditor : Editor
{
    bool debugMode = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        FactorySwitch eventButton = (FactorySwitch)target;
        
        
        debugMode = GUILayout.Toggle(debugMode, "Debug toggle");

        GUILayout.Space(10);

        if (GUILayout.Button("Update seats"))
        {
            
            
            var seats = GameObject.FindObjectsByType<VRCStation>(FindObjectsSortMode.None);
            Collider[] seatColliders= new Collider[seats.Length];
            for (int i = 0; i < seats.Length; i++)
            {
                seatColliders[i] = seats[i].GetComponent<Collider>();
            }
            eventButton.ObjectsToToggle = seatColliders;
            
            if(debugMode)
            {
                foreach (var collider in seatColliders)
                {
                    collider.transform.GetChild(2).gameObject.SetActive(true);
                }
            }else 
            {
                foreach (var collider in seatColliders)
                {
                    collider.transform.GetChild(2).gameObject.SetActive(false);
                }
            }
        }
    }
}
#endif


public class FactorySwitch : UdonSharpBehaviour
{
    
    public bool state = false;
    Transform switchTransform;
    
    public Collider[] ObjectsToToggle;
    
    public override void Interact()
    {
        state = !state;
        UpdateVisuals();
        foreach (Collider o in ObjectsToToggle)
        {
            if (o != null)
            {
                o.enabled = state;
            }
            
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
