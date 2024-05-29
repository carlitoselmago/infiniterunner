using UnityEngine;
using System.Collections;

public class ActivateAllDisplays : MonoBehaviour
{
    void Start()
    {
        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
        // Check if additional displays are available and activate each.

        if (Display.displays.Length > 1)
        {
            Debug.Log("More than one display connected. Required activation of output script.");
        }
        else
        {
            Debug.Log("Only main display has been detected.");
        }

        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }

    void Update()
    {

    }
}