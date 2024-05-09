using UnityEngine;
using System.Collections;

public class flickering : MonoBehaviour
{
    public Light pointLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float minWaitTime = 0.1f;
    public float maxWaitTime = 0.5f;

    private void Start()
    {
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            // Wait for random time
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            // Set random intensity
            pointLight.intensity = Random.Range(minIntensity, maxIntensity);

            // Wait for a short time
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

            // Reset intensity to its original value
            pointLight.intensity = maxIntensity;
        }
    }
}
