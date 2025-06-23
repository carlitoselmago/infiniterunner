using UnityEngine;

public class RotateY : MonoBehaviour
{
    public GameObject rotatedObject;         // The object you want to rotate
    public Transform player;                 // Reference to the player (dynamic center of rotation)
    public float rotationSpeed = 90.0f;      // Degrees per second
    public float rotationDegree = 90.0f;     // Total degrees to rotate

    private float totalRotated = 0f;

    void Update()
    {
        // Compute this frame’s rotation
        float rotationThisFrame = rotationSpeed * Time.deltaTime;

        // Prevent overshooting
        if (totalRotated + rotationThisFrame > rotationDegree)
        {
            rotationThisFrame = rotationDegree - totalRotated;
        }

        // Rotate around the player's current position, on Y axis
        rotatedObject.transform.RotateAround(player.position, Vector3.up, rotationThisFrame);
        totalRotated += rotationThisFrame;

        // Stop once desired rotation is reached
        if (totalRotated >= rotationDegree)
        {
            Debug.Log("Stop Rotation");
            this.enabled = false;
        }
    }
}