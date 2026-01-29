/*using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ForceFocus : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    void Start()
    {
        Application.runInBackground = true;

        // Use the exact window title of your Unity game
        string windowTitle = "Infinite Runner";
        IntPtr hWnd = FindWindow(null, windowTitle);

        if (hWnd != IntPtr.Zero)
        {
            SetForegroundWindow(hWnd);
        }
    }
}*/