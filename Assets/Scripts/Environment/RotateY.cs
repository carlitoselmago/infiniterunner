using UnityEngine;

public class RotateY : MonoBehaviour, IResettable
{
    public GameObject rotatedObject;         // The object to rotate
    public Transform player;                 // The player (dynamic Z/Y, fixed X)
    public float rotationSpeed = 90.0f;      // Degrees per second
    public float rotationDegree = 90.0f;     // Total degrees to rotate
    public int rotateDirection = 1;          // +1 for right, -1 for left

    private float totalRotated = 0f;
    private Vector3 rotationCenter;

    void OnEnable()
    {
        // Store rotation center using fixed X
        rotationCenter = new Vector3(0f, player.position.y, player.position.z);
        rotatedObject.transform.localRotation = Quaternion.identity;
    }

    void Update()
    {
        if (PlayerMove.onForklift) return;
        if (!PlayerMove.isDead)
        {
            // Compute this frame’s rotation
            float rotationThisFrame = rotationSpeed * Time.deltaTime;

            // Prevent overshooting
            if (totalRotated + rotationThisFrame > rotationDegree)
                rotationThisFrame = rotationDegree - totalRotated;

            // Apply direction
            float actualRotation = rotationThisFrame * rotateDirection;

            // Rotate around the fixed center on the Y axis
            rotatedObject.transform.RotateAround(rotationCenter, Vector3.up, actualRotation);
            totalRotated += rotationThisFrame;

            // Stop once desired rotation is reached
            if (totalRotated >= rotationDegree)
            {
                Debug.Log($"{name}: Stop Rotation");
                this.enabled = false;
            }
        } else
        {
            Debug.Log($"{name}: Rotation stopped after player's death");
            this.enabled = false;
        }
    }

    public void ResetState()
    {
        totalRotated = 0f;
        this.enabled = true;
        rotatedObject.transform.localRotation = Quaternion.identity;
    }
}