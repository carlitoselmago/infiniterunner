using UnityEngine;

public class FpsSpikeLogger : MonoBehaviour
{
    [Header("Low FPS condition")]
    public float fpsThreshold = 20f;
    public float requiredDuration = 3f; // seconds

    private float timeBelowThreshold;
    private bool spikeLogged;

    void Update()
    {
        float fps = 1f / Time.unscaledDeltaTime;

        if (fps < fpsThreshold)
        {
            timeBelowThreshold += Time.unscaledDeltaTime;

            if (!spikeLogged && timeBelowThreshold >= requiredDuration)
            {
                spikeLogged = true;
                if (!PlayerMove.idle)
                    Debug.Log(
                        $"LOW FPS EVENT | < {fpsThreshold} FPS for {requiredDuration} s"
                    );
            }
        }
        else
        {
            // Recovery: reset state
            timeBelowThreshold = 0f;
            spikeLogged = false;
        }
    }
}