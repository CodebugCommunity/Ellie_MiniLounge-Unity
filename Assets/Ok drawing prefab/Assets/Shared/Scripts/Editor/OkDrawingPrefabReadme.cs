
#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class OkDrawingPrefabReadme : ScriptableObject { }

[CustomEditor(typeof(OkDrawingPrefabReadme))]
public class OkDrawingPrefabReadmeEditor : Editor
{
    public static readonly string message = @"

                  ███       █      █
               █         █    █   █ 
               █         █    ██  
               █         █    █   █ 
                  ███       █      █
     
        Drawing prefab by Kenoli
           discord.gg/MzDST32
          ———————————
                      Version 1
                 January 2022

";
    public static int sourceIndex;
    public static int newIndex;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.TextField(message, GUILayout.Height(230.0f));

        GUILayout.Space(27.0f);
        GUILayout.Label("• Having multiple drawings requires duplicating the prefab.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(3.0f);
        GUILayout.Label("• Click the button below to create duplicates.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(3.0f);
        GUILayout.Label("• Use only one of each prefab.", EditorStyles.wordWrappedLabel);

        GUILayout.Space(20.0f);
        var found = AssetDatabase.FindAssets("\"Ok drawing prefab\" t:prefab");
        if (found.Length == 0)
        {
            GUILayout.Space(20.0f);
            GUILayout.Label("ERROR: NO PREFABS FOUND", EditorStyles.wordWrappedLabel);
            return;
        }

        GUILayout.Label("Current prefabs:", EditorStyles.boldLabel);
        for (int i = 0; i < found.Length; i++)
        {
            var guid = found[i];
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var assetName = Path.GetFileNameWithoutExtension(assetPath);
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            EditorGUILayout.ObjectField(obj, typeof(GameObject), false);
        }

        var source = AssetDatabase.GUIDToAssetPath(found[0]);
        var n = Path.GetFileNameWithoutExtension(source);
        n = new string(n.Where(p => char.IsDigit(p)).ToArray());
        sourceIndex = int.Parse(n);
        newIndex = found.Length + 1;

        if (newIndex > 9)
        {
            GUILayout.Space(20.0f);
            GUILayout.Label("ERROR: TOO MANY PREFABS", EditorStyles.wordWrappedLabel);
            return;
        }

        var dir = Directory.GetParent(source).ToString().Replace("\\", "/");
        var dest = dir + "Ok drawing prefab " + newIndex + ".prefab";
        var dataPath = Application.dataPath;
        dataPath = dataPath.Substring(0, dataPath.Length - 6);
        var newDir = dir.Substring(0, dir.Length - 1) + newIndex;

        if (AssetExists(newDir))
        {
            GUILayout.Space(20.0f);
            GUILayout.Label("ERROR: TARGET PREFAB ALREADY EXISTS", EditorStyles.wordWrappedLabel);
            return;
        }

        GUILayout.Space(5.0f);
        if (GUILayout.Button("Create a duplicate", GUILayout.Width(220.0f), GUILayout.Height(27.0f)))
        {
            CopyDirectoryDeep(dataPath + dir, dataPath + newDir);

            var newPrefabPath = newDir + "/Ok drawing prefab " + newIndex + ".prefab";
            var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath);
            SetPrefabLayer(newPrefab, 22 + newIndex);
            AssetDatabase.SaveAssets();
        }
    }

    public void SetPrefabLayer(GameObject newPrefab, int newLayer)
    {
        var drawingObjects = newPrefab.GetComponentsInChildren<EventTrigger>(true);
        var projectors = newPrefab.GetComponentsInChildren<Projector>(true);
        var cameras = newPrefab.GetComponentsInChildren<Camera>(true);

        foreach (var item in drawingObjects)
        {
            item.gameObject.layer = newLayer;
        }

        foreach (var item in projectors)
        {
            item.gameObject.layer = newLayer;
            item.ignoreLayers = ~(1 << newLayer);
        }

        foreach (var item in cameras)
        {
            if (item.name.Contains("Merge camera") || item.name.Contains("Initializer camera") || item.name.Contains("Synchronizer"))
            {
                item.cullingMask = 1;
            }
            else if (item.name.Contains("Render texture color"))
            {
                item.cullingMask = (1 << newLayer);
            }
            else if (item.name.Contains("color picker"))
            {
                item.cullingMask = 0x3FFFFF;
            }
            else if (item.name.Contains("Screenshot"))
            {
                item.cullingMask = 0x7FFFFF & ~(1 << 12 | 1 << 19);
            }
        }
    }

    public static bool AssetExists(string assetPath)
    {
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrWhiteSpace(guid))
        {
            return false;
        }

        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
        if (assetType == null || string.IsNullOrWhiteSpace(assetType.ToString()))
        {
            return false;
        }

        return true;
    }

    public static void CopyDirectoryDeep(string sourcePath, string destinationPath)
    {
        try
        {
            AssetDatabase.StartAssetEditing();

            CopyDirectoryRecursively(sourcePath, destinationPath);

            List<string> metaFiles = GetFilesRecursively(destinationPath, (f) => f.EndsWith(".meta"));
            List<(string originalGuid, string newGuid)> guidTable = new List<(string originalGuid, string newGuid)>();

            foreach (string metaFile in metaFiles)
            {
                StreamReader file = new StreamReader(metaFile);
                file.ReadLine();
                string guidLine = file.ReadLine();
                file.Close();
                string originalGuid = guidLine.Substring(6, guidLine.Length - 6);
                string newGuid = GUID.Generate().ToString().Replace("-", "");
                guidTable.Add((originalGuid, newGuid));
            }

            List<string> allFiles = GetFilesRecursively(destinationPath);

            foreach (string fileToModify in allFiles)
            {
                if (Path.GetExtension(fileToModify) == ".png")
                {
                    continue;
                }

                string content = File.ReadAllText(fileToModify);

                foreach (var guidPair in guidTable)
                {
                    content = content.Replace(guidPair.originalGuid, guidPair.newGuid);
                }

                File.WriteAllText(fileToModify, content);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
    }

    private static void CopyDirectoryRecursively(string sourceDirName, string destDirName)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        DirectoryInfo[] dirs = dir.GetDirectories();

        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            temppath = temppath.Replace("Ok drawing prefab " + sourceIndex, "Ok drawing prefab " + newIndex);
            file.CopyTo(temppath, false);
        }

        foreach (DirectoryInfo subdir in dirs)
        {
            string temppath = Path.Combine(destDirName, subdir.Name);
            CopyDirectoryRecursively(subdir.FullName, temppath);
        }
    }

    private static List<string> GetFilesRecursively(string path, Func<string, bool> criteria = null, List<string> files = null)
    {
        if (files == null)
        {
            files = new List<string>();
        }

        files.AddRange(Directory.GetFiles(path).Where(f => criteria == null || criteria(f)));

        foreach (string directory in Directory.GetDirectories(path))
        {
            GetFilesRecursively(directory, criteria, files);
        }

        return files;
    }
}

#endif
