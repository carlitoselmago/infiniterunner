using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinStreakUI : MonoBehaviour
{
    [SerializeField] private List<Image> dots;
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private Color flashColor = Color.cyan;
    [SerializeField] private float hideDelay = 0.5f;
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float popDuration = 0.15f;

    private Coroutine hideRoutine;
    private List<Vector3> originalScales = new List<Vector3>();

    void Start()
    {
        // Store initial scales
        foreach (var dot in dots)
            originalScales.Add(dot.transform.localScale);

        // Hide UI at start
        gameObject.SetActive(false);
        ResetDots();
    }

    public void UpdateDots(int streakCount, int streakStart, int streakMax)
    {
        if (dots == null || dots.Count == 0) return;

        if (streakCount >= streakStart)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            int activeCount = Mathf.Clamp(streakCount - streakStart, 0, dots.Count);

            for (int i = 0; i < dots.Count; i++)
            {
                if (i < activeCount)
                {
                    if (dots[i].color != activeColor)
                    {
                        dots[i].color = activeColor;
                        StartCoroutine(PopDot(dots[i]));
                    }
                }
                else
                {
                    dots[i].color = inactiveColor;
                    dots[i].transform.localScale = originalScales[i];
                }
            }

            // Reset hide timer
            if (hideRoutine != null)
                StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(HideAfterDelay());
        }
    }

    public void PlayAchievementFlash()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float flashTime = 0.5f;

        float t = 0f;
        while (t < flashTime)
        {
            t += Time.deltaTime;
            float pulse = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.2f;

            foreach (var dot in dots)
            {
                dot.color = Color.Lerp(activeColor, flashColor, Mathf.PingPong(t * 2f, 1f));
                dot.transform.localScale = originalScales[dots.IndexOf(dot)] * pulse;
            }

            yield return null;
        }

        ResetDots();

        // Keep visible for a short time before hiding
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator PopDot(Image dot)
    {
        int index = dots.IndexOf(dot);
        Vector3 baseScale = originalScales[index];
        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            float scale = Mathf.Lerp(popScale, 1f, t);
            dot.transform.localScale = baseScale * scale;
            yield return null;
        }

        dot.transform.localScale = baseScale;
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        gameObject.SetActive(false);
    }

    private void ResetDots()
    {
        for (int i = 0; i < dots.Count; i++)
        {
            dots[i].color = inactiveColor;
            dots[i].transform.localScale = originalScales[i];
        }
    }

    public void ResetUI()
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        ResetDots();
        gameObject.SetActive(false);
    }
}
