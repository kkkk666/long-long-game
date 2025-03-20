using UnityEngine;

public class DisplayCozyCanvas : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
          GameObject cozyManager = GameObject.Find("CozyManager");
        if (cozyManager != null)
        {
           //get the child object named Canvas
           GameObject canvas = cozyManager.transform.Find("Canvas").gameObject;
           if (canvas != null && canvas.activeSelf == false)
           {
            //set the canvas to active
            canvas.SetActive(true);
           }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
