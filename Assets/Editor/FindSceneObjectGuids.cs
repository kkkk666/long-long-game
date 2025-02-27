using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindSceneObjectGuids : EditorWindow
{
    [MenuItem("Tools/Find SceneObjectGuid Components")]
    static void FindComponents()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        List<string> prefabsWithGuid = new List<string>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                var guidComponents = prefab.GetComponentsInChildren<Unity.Tutorials.Core.SceneObjectGuid>(true);
                if (guidComponents != null && guidComponents.Length > 0)
                {
                    prefabsWithGuid.Add(path);
                    Debug.Log($"Found SceneObjectGuid in: {path} - Component count: {guidComponents.Length}");
                }
            }
        }
        
        Debug.Log($"Found {prefabsWithGuid.Count} prefabs with SceneObjectGuid components");
    }
} 