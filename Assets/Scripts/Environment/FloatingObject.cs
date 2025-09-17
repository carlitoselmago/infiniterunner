/*
 using UnityEngine;

public class FloatingObject : MonoBehaviour, IResettable
{
    public Transform mapTransform;           // Assign your scrolling map here

    public float waterHeight = 0f;           // Base Y level of the water
    public float floatStrength = 1f;         // Vertical smoothing strength
    public float bobSpeed = 1f;              // Speed of bobbing
    public float bobAmount = 0.05f;          // Amount of bobbing
    public float tiltAmount = 2f;            // Max rocking tilt in degrees

    private Quaternion startRot;

    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;

    void Awake()
    {
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
    }

    void Start()
    {
        if (!mapTransform)
        {
            Debug.LogWarning($"{name} has no mapTransform assigned!");
            enabled = false;
            return;
        }

        startRot = transform.rotation;
    }

    void Update()
    {
        // Start from current position (section design handles base placement)
        Vector3 pos = transform.position;

        // Bobbing motion on Y axis
        float bobOffset = Mathf.Sin(Time.time * bobSpeed + transform.position.x) * bobAmount;
        float targetY = waterHeight + bobOffset;
        pos.y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * floatStrength);

        transform.position = pos;

        // Gentle rocking rotation
        float tiltX = Mathf.Sin(Time.time * bobSpeed * 0.7f) * tiltAmount;
        float tiltZ = Mathf.Cos(Time.time * bobSpeed * 0.9f) * tiltAmount;
        Quaternion targetRot = Quaternion.Euler(tiltX, startRot.eulerAngles.y, tiltZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 0.5f);
    }

    public void ResetState()
    {
        transform.localPosition = initialLocalPos;
        transform.localRotation = initialLocalRot;
        startRot = transform.rotation;
    }
}*/