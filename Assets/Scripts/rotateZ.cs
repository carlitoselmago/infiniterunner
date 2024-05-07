using UnityEngine;

public class RotateZ : MonoBehaviour
{
    public float rotationSpeed = 90.0f; // Speed of rotation in degrees per second

    void Update()
    {
        // Rotate the object around its local Z-axis at a constant speed
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
