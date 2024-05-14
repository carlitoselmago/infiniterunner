using UnityEngine;

public class FlashingLight : MonoBehaviour
{
    public float flashInterval = 1.0f; // Time interval between flashes in seconds
    public float onDuration = 0.5f; // Duration the light is on during each flash

    private bool isLightOn = false;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // Check if it's time to toggle the light
        if (timer >= flashInterval)
        {
            isLightOn = !isLightOn;
            GetComponent<Light>().enabled = isLightOn;
            timer = 0f;

            // If the light is now on, schedule turning it off
            if (isLightOn)
            {
                Invoke("TurnLightOff", onDuration);
            }
        }
    }

    void TurnLightOff()
    {
        // Turn the light off after the specified duration
        GetComponent<Light>().enabled = false;
    }
}