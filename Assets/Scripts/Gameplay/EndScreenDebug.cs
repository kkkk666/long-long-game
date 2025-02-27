using UnityEngine;

public class DebugEndScreen : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("End Screen object name: " + gameObject.name);
        Debug.Log("End Screen object path: " + GetFullPath(gameObject));
        
        // Make sure it's initially inactive
        gameObject.SetActive(false);
    }
    
    // Helper method to get the full path of an object in the hierarchy
    private string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
}