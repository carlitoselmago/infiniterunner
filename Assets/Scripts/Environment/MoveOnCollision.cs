using System.Collections;
using UnityEngine;

public class MoveOnCollision : MonoBehaviour
{
    private bool hasMoved = false; // Flag to track if the object has already moved

    public IEnumerator MoveObject(float moveAmount, float moveDuration)
    {
        Debug.Log("Movement active");
        // Calculate the target position based on the moveAmount
        Vector3 targetPosition = transform.localPosition + new Vector3(moveAmount, 0f, 0f);

        // Store the starting time
        float startTime = Time.time;

        // Loop until the elapsed time reaches the moveDuration
        while (Time.time - startTime < moveDuration)
        {
            // Calculate the interpolation factor based on the elapsed time
            float t = (Time.time - startTime) / moveDuration;

            // Interpolate the position between the starting position and the target position
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, t);

            // Yield and wait for the next frame
            yield return null;
        }

        // Ensure the object reaches the exact target position
        transform.localPosition = targetPosition;

        // Set the flag to true indicating the object has moved
        hasMoved = true;
    }
}