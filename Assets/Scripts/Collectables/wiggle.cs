using UnityEngine;

public class wiggle : MonoBehaviour
{
    public float amplitude = 0.5f;    // Amplitude of the float.
    public float frequency = 0.7f;    // Speed of the float.
    public float rotationSpeed = 50f; // Speed of the rotation.

    private Vector3 startPos;
    private bool initialized = false;

    public void Initialize(Vector3 position)
    {
        startPos = position;
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        // Float up/down with a sine wave.
        float tempPos = amplitude * Mathf.Sin(Time.time * frequency);
        transform.localPosition = new Vector3(startPos.x, startPos.y + tempPos, startPos.z);

        // Rotate around the Y-axis.
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}