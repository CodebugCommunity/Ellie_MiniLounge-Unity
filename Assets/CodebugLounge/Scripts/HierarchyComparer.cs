using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Linq;

// We need the UnityEditor namespace for the custom inspector,
// but we wrap it in a preprocessor directive to prevent build errors.
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A utility to synchronize the active state of GameObjects in one hierarchy
/// based on the state of correspondingly-named objects in another hierarchy.
/// This tool matches children based on their full relative path to handle duplicate names.
/// </summary>
public class HierarchySync : MonoBehaviour
{
    [Header("Hierarchy References")]
    [Tooltip("The hierarchy to use as the 'source of truth'.")]
    public GameObject sourceObjectA;

    [Tooltip("The hierarchy to modify, making its active states match the source.")]
    public GameObject targetObjectB;

    /// <summary>
    /// Synchronizes the active states from hierarchy A to hierarchy B.
    /// </summary>
    public void SyncFromAToB()
    {
        if (!ValidateInputs()) return;

        Debug.Log($"--- Starting Sync: Source '{sourceObjectA.name}' -> Target '{targetObjectB.name}' ---");
        // 1. Create a map of the source hierarchy's active states, using the relative path as the key.
        Dictionary<string, bool> sourceStateMap = BuildStateMap(sourceObjectA);

        // 2. Apply this state map to the target hierarchy.
        ApplyStateMapToHierarchy(targetObjectB, sourceObjectA, sourceStateMap, $"Sync {sourceObjectA.name} to {targetObjectB.name}");
    }

    /// <summary>
    /// Synchronizes the active states from hierarchy B to hierarchy A.
    /// </summary>
    public void SyncFromBToA()
    {
        if (!ValidateInputs()) return;
        
        Debug.Log($"--- Starting Sync: Source '{targetObjectB.name}' -> Target '{sourceObjectA.name}' ---");
        // 1. Create a map of the source hierarchy's active states.
        Dictionary<string, bool> sourceStateMap = BuildStateMap(targetObjectB);
        
        // 2. Apply this state map to the target hierarchy.
        ApplyStateMapToHierarchy(sourceObjectA, targetObjectB, sourceStateMap, $"Sync {targetObjectB.name} to {sourceObjectA.name}");
    }
    
    /// <summary>
    /// Builds a dictionary mapping the relative path of each descendant to its active state.
    /// </summary>
    private Dictionary<string, bool> BuildStateMap(GameObject root)
    {
        var map = new Dictionary<string, bool>();
        var allTransforms = root.GetComponentsInChildren<Transform>(true);

        foreach (var transform in allTransforms)
        {
            if (transform == root.transform) continue; // Skip the root itself

            //string path = GetRelativePath(transform, root.transform);
            if (!map.ContainsKey(transform.name))
            {
                // Add the path and its active state to the map
                map.Add(transform.name, transform.gameObject.activeSelf);
                //Debug.Log($"Added '{path}' with active state {transform.gameObject.activeSelf} from source '{root.name}'.", transform.gameObject);
            }
            else if (!map.ContainsKey(transform.name)) // Check for duplicate paths
            {
                map.Add(transform.name, transform.gameObject.activeSelf);
                //Debug.Log($"Added '{path}' with active state {transform.gameObject.activeSelf} from source '{root.name}'.", transform.gameObject);
            }
            else
            {
                Debug.LogWarning($"Duplicate path found in source '{root.name}': '{transform.name}'. The first one found will be used.", transform.gameObject);
            }
        }
        return map;
    }
    
    /// <summary>
    /// Modifies the target hierarchy based on the provided state map.
    /// </summary>
    private void ApplyStateMapToHierarchy(GameObject targetRoot, GameObject sourceRoot, Dictionary<string, bool> sourceStateMap, string undoName)
    {
        var allTargetTransforms = targetRoot.GetComponentsInChildren<Transform>(true);
        int enabledCount = 0;
        int disabledCount = 0;
        int disabledinSourceCount = 0;
        
        // ** CRITICAL FOR UNDO **
        // Record the state of all GameObjects we might modify *before* we loop through them.
        // This creates a single, reliable Undo operation.
        #if UNITY_EDITOR
        Undo.RecordObjects(allTargetTransforms.Select(t => t.gameObject).ToArray(), undoName);
        #endif

        
        Debug.Log($"Processing .");
        foreach (var targetTransform in allTargetTransforms)
        {
            
            if (targetTransform == targetRoot.transform) continue; // Skip the root
            
            string path = GetRelativePath(targetTransform, targetRoot.transform);
            path = targetTransform.name;
            //Debug.Log($"Processing '{path}' in target '{targetRoot.name}'.");
            
            // Try to find a matching object in the source map
            if (sourceStateMap.TryGetValue(path, out bool sourceIsActive))
            {
                // Match found. Sync the active state if it's different.
                if (targetTransform.gameObject.activeSelf != sourceIsActive)
                {
                    targetTransform.gameObject.SetActive(sourceIsActive);
                    //sourceRoot.transform.Find(path)?.gameObject.SetActive(!sourceIsActive);
                    if (sourceIsActive) enabledCount++;
                    else disabledCount++;
                }else
                {
                    
                    //Debug.Log($"Disabling '{path}' in source");
                }
                
                foreach (Transform child in sourceRoot.GetComponentsInChildren<Transform>()) {
                    if (child.name == path)
                    {
                        child.gameObject.SetActive(!sourceIsActive);
                        disabledinSourceCount++;
                    }
                        
                }
            }
            else
            {
                
                continue;
                // No match found in the source hierarchy, so disable this object.
                if (targetTransform.gameObject.activeSelf)
                {
                    targetTransform.gameObject.SetActive(false);
                    disabledCount++;
                    Debug.Log($"'{path}' not found in source '{sourceRoot.name}', disabling in target '{targetRoot.name}'.", targetTransform.gameObject);
                }
            }
        }
        
        Debug.Log($"--- Sync Complete --- \nEnabled: {enabledCount}, Disabled: {disabledCount}.  Disabled in source: {disabledinSourceCount}.Total objects processed: {allTargetTransforms.Length - 1}.");
    }

    /// <summary>
    /// Calculates the relative path of a transform from a given ancestor (root).
    /// </summary>
    private string GetRelativePath(Transform child, Transform root)
    {
        if (child == root) return "";

        var pathParts = new List<string>();
        Transform current = child;

        while (current != null && current != root)
        {
            pathParts.Add(current.name);
            current = current.parent;
        }

        pathParts.Reverse();
        return string.Join("/", pathParts);
    }

    private bool ValidateInputs()
    {
        if (sourceObjectA == null || targetObjectB == null)
        {
            Debug.LogError("Both Source (A) and Target (B) GameObjects must be assigned in the Inspector.", this);
            return false;
        }
        return true;
    }
}


// ====================================================================================
// CUSTOM EDITOR
// This code only compiles in the Unity Editor and handles drawing the custom inspector.
// ====================================================================================
#if UNITY_EDITOR
[CustomEditor(typeof(HierarchySync))]
public class HierarchySyncEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields (sourceObjectA and targetObjectB).
        DrawDefaultInspector();

        HierarchySync script = (HierarchySync)target;

        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "These buttons sync the active state of children from the Source to the Target. " +
            "It matches children based on their full path. Any child in the Target that doesn't exist in the Source will be disabled. " +
            "This action is fully undoable (Ctrl+Z).", MessageType.Info);
        
        EditorGUILayout.Space(5);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(10, 10, 8, 8),
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };

        // Button 1: Sync from A to B
        if (GUILayout.Button("Sync Active States from A -> B", buttonStyle))
        {
            script.SyncFromAToB();
        }

        // Button 2: Sync from B to A
        if (GUILayout.Button("Sync Active States from B -> A", buttonStyle))
        {
            script.SyncFromBToA();
        }
    }
}
#endif