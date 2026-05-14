using UnityEngine;

public class FPSTarget : MonoBehaviour
{
    public int target = 60;
    void Awake()
    {
        QualitySettings.vSyncCount = 1; // used to be 0
        Application.targetFrameRate = target;
    }
}