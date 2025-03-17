using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class ComponentLister : MonoBehaviour
{
    [SerializeField] private bool listOnStart = true;
    [SerializeField] private bool listOnUpdate = false;
    [SerializeField] private bool listParameters = false;

    private void Start()
    {
        if (listOnStart)
        {
            ListComponents();
        }
    }

    private void Update()
    {
        if (listOnUpdate)
        {
            ListComponents();
        }
    }

    public void ListComponents()
    {
        Debug.Log($"=== Components on {gameObject.name} ===");
        
        // Get all components
        Component[] components = GetComponents<Component>();
        
        foreach (Component component in components)
        {
            if (component != null)
            {
                Debug.Log($"Component Name: {component.GetType().Name}");
                Debug.Log($"Full Type: {component.GetType().FullName}");
                Debug.Log($"Assembly: {component.GetType().Assembly.GetName().Name}");

                if (listParameters)
                {
                    Debug.Log("Parameters:");
                    // Get all properties
                    PropertyInfo[] properties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (PropertyInfo property in properties)
                    {
                        try
                        {
                            object value = property.GetValue(component);
                            Debug.Log($"  {property.Name}: {value}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log($"  {property.Name}: [Error reading value: {e.Message}]");
                        }
                    }

                    // Get all fields
                    FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        try
                        {
                            object value = field.GetValue(component);
                            Debug.Log($"  {field.Name}: {value}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log($"  {field.Name}: [Error reading value: {e.Message}]");
                        }
                    }
                }

                Debug.Log("---");
            }
        }
    }
} 