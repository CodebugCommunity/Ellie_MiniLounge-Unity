
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
            Undo.RecordObject(resetter, "Update seats");
            
            var interactables = GameObject.FindObjectsByType<VRCPickup>(FindObjectsSortMode.None);
            int count = interactables.Length;
            GameObject[] interactableObjs = new GameObject[count];
            Vector3[] positions = new Vector3[count];
            Quaternion[] rotations = new Quaternion[count];

            for (int i = 0; i < count; i++)
            {
                interactableObjs[i] = interactables[i].gameObject;
                positions[i] = interactables[i].transform.position;
                rotations[i] = interactables[i].transform.rotation;
            }
            
            resetter.objectsToReset = interactableObjs;
            resetter.startingPositions = positions;
            resetter.startingRotations = rotations;
            
            EditorUtility.SetDirty(resetter);
        }
    }
}
#endif

public class ObjectResetter : UdonSharpBehaviour
{
    [SerializeField] public GameObject[] objectsToReset;
    [SerializeField] public Vector3[] startingPositions;
    [SerializeField] public Quaternion[] startingRotations;
    
    public override void Interact()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ResetObjects));   
        Debug.Log("0Reset objects to starting positions and rotations.");
    }
    public void ResetObjects()
    {
        Debug.Log("1Reset objects to starting positions and rotations.");
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
