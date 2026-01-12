using UnityEngine;

public class ParachuteDescent : MonoBehaviour, IResettable
{
    // Descent Settings
    public float targetLocalY = -3.67f;
    public float fallSpeed = 1.5f;

    // Float & Rotate Settings
    private float amplitude = 0.5f;
    private float frequency = 1f;
    private float rotationSpeed = 50f;

    private bool isDescending = true;
    private Vector3 startLocalPosition;
    private Vector3 finalLocalPosition;
    private float floatStartTime;

    void Start()
    {
        // Cache editor placement RELATIVE to the template
        startLocalPosition = transform.localPosition;
    }

    void Update()
    {
        if (isDescending)
        {
            Vector3 current = transform.localPosition;

            if (current.y > targetLocalY)
            {
                float newY = Mathf.MoveTowards(
                    current.y,
                    targetLocalY,
                    fallSpeed * Time.deltaTime
                );

                transform.localPosition = new Vector3(
                    current.x,
                    newY,
                    current.z
                );
            }
            else
            {
                transform.localPosition = new Vector3(
                    current.x,
                    targetLocalY,
                    current.z
                );

                finalLocalPosition = transform.localPosition;
                floatStartTime = Time.time;
                isDescending = false;
            }
        }
        else
        {
            float tempY = amplitude * Mathf.Sin(
                (Time.time - floatStartTime) * frequency
            );

            transform.localPosition = new Vector3(
                finalLocalPosition.x,
                finalLocalPosition.y + tempY,
                finalLocalPosition.z
            );

            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }

    public void StopRotation()
    {
        if (!isDescending)
            rotationSpeed = 0f;
    }

    public void ResetState()
    {
        transform.localPosition = startLocalPosition;
        rotationSpeed = 50f;
        isDescending = true;
        floatStartTime = 0f;
        finalLocalPosition = transform.localPosition;
    }
}
