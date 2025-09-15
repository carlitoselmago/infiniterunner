using UnityEngine;
using System.Collections;

public class Flickering : MonoBehaviour
{
    private Light componentLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float minWaitTime = 0.1f;
    public float maxWaitTime = 0.5f;

    public bool energyShield = false;
    public Renderer shield;
    public bool isFlickering = true;

    private Coroutine flickerCoroutine;

    private void Awake()
    {
        componentLight = GetComponent<Light>();

        if (energyShield)
            shield = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        if (componentLight != null)
            flickerCoroutine = StartCoroutine(Flicker());
    }

    private void OnDisable()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            // Wait for random time
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            if (isFlickering)
            {
                // Set random intensity
                componentLight.intensity = Random.Range(minIntensity, maxIntensity);

                if (energyShield)
                    shield.enabled = !shield.enabled;

                // Wait for a short time
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

                // Reset intensity to its original value
                componentLight.intensity = maxIntensity;
            }
        }
    }

    public void ToggleFlickering()
    {
        isFlickering = !isFlickering;
        if (!isFlickering && shield)
            shield.enabled = true; // Ensure renderer is enabled when flickering is off
    }
}