
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

#if UNITY_EDITOR
using UnityEditor;
using VRC.SDK3.Components;

[CustomEditor(typeof(ObjectResetter))]
public class ObjectResetterEditor : Editor
{
    bool debugMode = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ObjectResetter resetter = (ObjectResetter)target;
        
        
        //debugMode = GUILayout.Toggle(debugMode, "Debug toggle");

        GUILayout.Space(10);

        if (GUILayout.Button("Update seats"))
        {
            
            
            var interactables = GameObject.FindObjectsByType<VRCPickup>(FindObjectsSortMode.None);
            GameObject[] interactableObjs= new GameObject[interactables.Length];
            for (int i = 0; i < interactables.Length; i++)
            {
                interactableObjs[i] = interactables[i].gameObject;
            }
            
            resetter.objectsToReset = interactableObjs;
            
            
        }
    }
}
#endif

public class ObjectResetter : UdonSharpBehaviour
{
    [SerializeField] public GameObject[] objectsToReset;
    [SerializeField] private Vector3[] startingPositions;
    [SerializeField] private Quaternion[] startingRotations;
    void Awake()
    {
        startingPositions = new Vector3[objectsToReset.Length];
        startingRotations = new Quaternion[objectsToReset.Length];
        
        for (int i = 0; i < objectsToReset.Length; i++)
        {
            startingPositions[i] = objectsToReset[i].transform.position;
            startingRotations[i] = objectsToReset[i].transform.rotation;
        }
    }
    
    public override void Interact()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ResetObjects));   
    }

    void ResetObjects()
    {
        for (int i = 0; i < objectsToReset.Length; i++)
        {
            objectsToReset[i].transform.position = startingPositions[i];
            objectsToReset[i].transform.rotation = startingRotations[i];
        }
        Debug.Log("Reset objects to starting positions and rotations.");
    }
}

struct TransformData
{
    public Vector3 position;
    public Quaternion rotation;
}
