using UnityEngine;

public class ParachuteDescent : MonoBehaviour, IResettable
{
    // Descent Settings
    public float targetY = -3.67f;
    public float fallSpeed = 1.5f;

    // Float & Rotate Settings
    private float amplitude = 0.5f;
    private float frequency = 1f;
    private float rotationSpeed = 50f;

    private bool isDescending = true;
    private Vector3 startLocal;
    private Vector3 finalPosition;
    private float floatStartTime;

    /*void Awake()
    {
        startLocal = transform.localPosition;
    }*/

    void Start()
    {
        startLocal = transform.localPosition;
        finalPosition = transform.localPosition;
    }
    /*
    void OnEnable()
    {
        ResetState();
    }*/

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

            // Rotate smoothly
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    public void StopRotation()
    {
        if (!isDescending)
            rotationSpeed = 0;
    }

    public void ResetState()
    {
        transform.localPosition = startLocal;
        rotationSpeed = 50f;
        isDescending = true;
        floatStartTime = 0f;
        finalPosition = transform.localPosition;
    }
}