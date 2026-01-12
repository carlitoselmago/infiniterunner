using UnityEngine;
using System.Collections;

public class LaneHighlight : MonoBehaviour
{
    [Header("References")]
    public Transform player;        // player transform
    public float forwardOffset = 5f; // in front of player
    public float yOffset = 0.05f;    // above ground

    [Header("Flash Settings")]
    public float maxIntensity = 3f;
    public float duration = 1f;

    private Renderer matRenderer;
    private Material mat;
    private Color baseEmission;
    private Coroutine routine;

    void Awake()
    {
        matRenderer = GetComponent<Renderer>();
        mat = matRenderer.material;
        mat.EnableKeyword("_EMISSION");
        baseEmission = mat.GetColor("_EmissionColor");
        matRenderer.enabled = false;
    }

    /// <summary>
    /// Flash the full lane area
    /// </summary>
    public void Flash()
    {
        // Track player Y + forward
        transform.position = new Vector3(
            0f,                         // centered horizontally
            player.position.y + yOffset,
            player.position.z + forwardOffset
        );

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        matRenderer.enabled = true;
        float t = 0f;

        // Fade in
        while (t < duration * 0.3f)
        {
            t += Time.deltaTime;
            mat.SetColor("_EmissionColor", baseEmission * Mathf.Lerp(0f, maxIntensity, t / (duration * 0.3f)));
            yield return null;
        }

        t = 0f;

        // Fade out
        while (t < duration * 0.7f)
        {
            t += Time.deltaTime;
            mat.SetColor("_EmissionColor", baseEmission * Mathf.Lerp(maxIntensity, 0f, t / (duration * 0.7f)));
            yield return null;
        }

        matRenderer.enabled = false;
        routine = null;
    }
}
