using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RemoveSceneObjectGuids : EditorWindow
{
    [MenuItem("Tools/Remove SceneObjectGuid Components")]
    static void RemoveComponents()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        List<string> modifiedPrefabs = new List<string>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                // Create a temporary instance to modify
                GameObject tempInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                var guidComponents = tempInstance.GetComponentsInChildren<Unity.Tutorials.Core.SceneObjectGuid>(true);
                
                if (guidComponents != null && guidComponents.Length > 0)
                {
                    bool modified = false;
                    
                    foreach (var component in guidComponents)
                    {
                        Object.DestroyImmediate(component);
                        modified = true;
                    }
                    
                    if (modified)
                    {
                        // Apply changes back to the prefab
                        PrefabUtility.SaveAsPrefabAsset(tempInstance, path);
                        modifiedPrefabs.Add(path);
                        Debug.Log($"Removed SceneObjectGuid components from: {path}");
                    }
                }
                
                // Clean up the temporary instance
                Object.DestroyImmediate(tempInstance);
            }
        }
        
        Debug.Log($"Modified {modifiedPrefabs.Count} prefabs by removing SceneObjectGuid components");
    }
} 