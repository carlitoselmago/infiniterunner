using UnityEngine;

public class ResettableObject : MonoBehaviour, IResettable
{
    public bool wokeOnEnable = false; // define if the object waits for player to be moved, or it should move already before
    private Vector3 startPos;
    private Quaternion startRot;
    private Rigidbody rb;

    void Start()
    {
        // Save the default local transform
        startPos = transform.localPosition;
        startRot = transform.localRotation;

        rb = GetComponent<Rigidbody>();
    }

    public void ResetState()
    {
        // Reset physics
        if (rb != null)
        {
            if (wokeOnEnable)
                rb.WakeUp();
            else
                rb.Sleep();
        }

        // Reset transform to original local position/rotation
        transform.localPosition = startPos;
        transform.localRotation = startRot;
    }
}
