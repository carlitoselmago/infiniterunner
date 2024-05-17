using UnityEngine;

public class ToggleShield : MonoBehaviour
{
    public Renderer shield;
    public float flashInterval = 0.25f;
    public float onDuration = 0.25f;

    private static bool isOn = false;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= flashInterval)
        {
            isOn = !isOn;
            shield.enabled = isOn;
            timer = 0f;
            if (isOn)
            {
                Invoke("TurnOff", onDuration);
            }
        }
    }

    void TurnOff()
    {
        shield.enabled = false;
    }
}