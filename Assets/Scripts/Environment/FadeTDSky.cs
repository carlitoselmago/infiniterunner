using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class FadeTDSky : MonoBehaviour, IResettable
{
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float minVisibleTime = 15f;
    [SerializeField] private float maxVisibleTime = 40f;

    private Material _material;
    private Color _baseColor;
    private Coroutine _fadeRoutine;

    void Awake()
    {
        _material = GetComponent<MeshRenderer>().material;
        _baseColor = _material.color;
    }

    void OnEnable()
    {
        // Reset alpha to 0 at start
        Color c = _baseColor;
        c.a = 0f;
        _material.color = c;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        // Fade in
        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeDuration));

        // Wait for a random interval
        float waitTime = Random.Range(minVisibleTime, maxVisibleTime);
        //Debug.Log(waitTime);
        yield return new WaitForSeconds(waitTime);

        // Fade out
        yield return StartCoroutine(FadeAlpha(1f, 0f, fadeDuration));

        // Deactivate object
        gameObject.SetActive(false);
    }

    private IEnumerator FadeAlpha(float start, float end, float duration)
    {
        float elapsed = 0f;
        Color c = _material.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(start, end, t);
            _material.color = c;
            yield return null;
        }

        // Ensure exact end alpha
        c.a = end;
        _material.color = c;
    }

    void OnDisable()
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
    }

    public void ResetState()
    {
        gameObject.SetActive(false);
    }
}
