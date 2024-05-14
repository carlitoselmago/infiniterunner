using UnityEngine;
using UnityEngine.UI;

public class BlinkRawImage : MonoBehaviour
{
    public float blinkInterval = 0.5f; // Time in seconds between blinks
    private RawImage rawImage;
    private float timer;
    private bool isVisible;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        if (rawImage == null)
        {
            Debug.LogError("No RawImage component found on this GameObject.");
            enabled = false;
            return;
        }

        isVisible = rawImage.enabled;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= blinkInterval)
        {
            isVisible = !isVisible;
            rawImage.enabled = isVisible;
            timer = 0f;
        }
    }
}
