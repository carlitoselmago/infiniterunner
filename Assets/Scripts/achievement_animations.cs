using UnityEngine;
using System.Collections;

public class AchievementAnimations : MonoBehaviour
{
    public float animationDuration = 1f; // Duration of the animation
    public AnimationCurve positionCurve; // Curve for position animation
    public AnimationCurve alphaCurve; // Curve for alpha animation

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 initialPosition;
    private bool isAnimating = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (rectTransform == null)
        {
            Debug.LogError("RectTransform component is missing.");
            return;
        }

        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component is missing. Adding one.");
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        initialPosition = rectTransform.anchoredPosition;
    }

    void OnEnable()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateIn());
        }
    }

    IEnumerator AnimateIn()
    {
        isAnimating = true;
        canvasGroup.alpha = 0;
        Vector2 startPosition = new Vector2(initialPosition.x, initialPosition.y - 100); // Start below the initial position
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);

            // Apply position animation with easing
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, initialPosition, positionCurve.Evaluate(t));
            // Apply alpha animation with easing
            canvasGroup.alpha = Mathf.Lerp(0, 1, alphaCurve.Evaluate(t));

            yield return null;
        }

        // Ensure final positions and alpha
        rectTransform.anchoredPosition = initialPosition;
        canvasGroup.alpha = 1;
        isAnimating = false;
        yield return new WaitForSeconds(4);
        StartCoroutine(ReverseFadeOut());
    }

    public IEnumerator ReverseFadeOut()
    {
        isAnimating = true;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);

            // Apply alpha animation with easing
            canvasGroup.alpha = Mathf.Lerp(1, 0, alphaCurve.Evaluate(t));

            yield return null;
        }

        // Ensure final alpha
        canvasGroup.alpha = 0;
        isAnimating = false;
        gameObject.SetActive(false);
    }
}
