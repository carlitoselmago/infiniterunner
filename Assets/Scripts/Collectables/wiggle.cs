using UnityEngine;

public class wiggle : MonoBehaviour
{
    public float amplitude = 0.5f;    // Amplitude of the float.
    public float frequency = 1f;      // Speed of the float.
    public float rotationSpeed = 50f; // Speed of the rotation.

    private Vector3 startPos;

   void Start()
    {
        // Record the starting local position of the object.
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Float up/down with a sine wave, preserving the local Z position.
        float tempPos = amplitude * Mathf.Sin(Time.time * frequency);
        transform.localPosition = new Vector3(startPos.x, startPos.y + tempPos, startPos.z);

        // Rotate around the Y-axis.
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}