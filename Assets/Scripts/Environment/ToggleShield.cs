using UnityEngine;
using System.Collections;

public class ToggleShield : MonoBehaviour
{
    public Renderer shield;
    public float flashInterval = 0.25f;
    public float onDuration = 0.25f;

    private bool isOn = false;
    private float timer = 0f;
    private Coroutine toggleCoroutine;

    private void OnEnable()
    {
        // Ensure the shield is always enabled when the script is activated
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
        }
        toggleCoroutine = StartCoroutine(ToggleShieldCoroutine());
    }

    private void OnDisable()
    {
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }
        shield.enabled = false; // Ensure the shield is off when the script is disabled
    }

    IEnumerator ToggleShieldCoroutine()
    {
        while (true)
        {
            shield.enabled = true;
            yield return new WaitForSeconds(onDuration);

            shield.enabled = false;
            yield return new WaitForSeconds(flashInterval - onDuration);
        }
    }
}
