using UnityEngine;

public class ParachuteDescent : MonoBehaviour
{
    [SerializeField] private float targetY = -3.67f;
    [SerializeField] private float fallSpeed = 1.5f;

    private bool isDescending = true;
    private wiggle wiggleScript;

    void Start()
    {
        wiggleScript = GetComponent<wiggle>();
        if (wiggleScript != null)
        {
            wiggleScript.enabled = false; // Prevent early activation
        }
    }

    void Update()
    {
        if (isDescending)
        {
            Vector3 currentPosition = transform.position;

            if (currentPosition.y > targetY)
            {
                float newY = Mathf.MoveTowards(currentPosition.y, targetY, fallSpeed * Time.deltaTime);
                transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
            }
            else
            {
                transform.position = new Vector3(currentPosition.x, targetY, currentPosition.z);
                isDescending = false;

                if (wiggleScript != null)
                {
                    wiggleScript.enabled = true;
                    wiggleScript.Initialize(transform.localPosition); // Pass in final position
                }
            }
        }
    }
}
