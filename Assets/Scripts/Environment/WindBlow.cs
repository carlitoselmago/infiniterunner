using UnityEngine;

public class WindBlow : MonoBehaviour
{
    private float swayAmount = 0.1f; // Amount of sway
    private float swaySpeed = 1f; // Speed of the sway

    private Vector3 startPos; // Starting local position of the cube

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float sway = Mathf.PerlinNoise(Time.time * swaySpeed, 0f) * 2f - 1f;
        Vector3 swayMovement = new Vector3(sway * swayAmount, 0f, 0f);
        transform.localPosition = startPos + swayMovement;
    }
}
