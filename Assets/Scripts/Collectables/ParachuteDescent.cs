using UnityEngine;

public class ParachuteDescent : MonoBehaviour
{
    // Descent Settings
    private float targetY = -3.67f;
    public float fallSpeed = 1.5f;

    // Float & Rotate Settings
    private float amplitude = 0.5f;
    private float frequency = 1f;
    private float rotationSpeed = 50f;

    private bool isDescending = true;
    private Vector3 finalPosition;
    private float floatStartTime;

    void Start()
    {
        finalPosition = transform.localPosition;
    }

    void Update()
    {
        if (isDescending)
        {
            Vector3 current = transform.position;

            if (current.y > targetY)
            {
                float newY = Mathf.MoveTowards(current.y, targetY, fallSpeed * Time.deltaTime);
                transform.position = new Vector3(current.x, newY, current.z);
            }
            else
            {
                // Lock final Y position, remember start time for sine wave
                transform.position = new Vector3(current.x, targetY, current.z);
                finalPosition = transform.localPosition;
                floatStartTime = Time.time;
                isDescending = false;
            }
        }
        else
        {
            // Wiggling phase
            float tempY = amplitude * Mathf.Sin((Time.time - floatStartTime) * frequency);
            transform.localPosition = new Vector3(finalPosition.x, finalPosition.y + tempY, finalPosition.z);

            // Optional: rotate smoothly
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
}