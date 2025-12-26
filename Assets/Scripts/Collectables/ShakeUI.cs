using UnityEngine;
using DG.Tweening;

public class ShakeUI : MonoBehaviour
{
    public RectTransform text;

    private Vector3 originalScale;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<RectTransform>();

        originalScale = text.localScale;
    }

    private void OnEnable()
    {
        text.DOKill();
        text.localScale = originalScale;

        text.DOPunchScale(
            punch: Vector3.one * 5.55f,   // how much it enlarges
            duration: 0.55f,              // fast attention grab
            vibrato: 20,
            elasticity: 5.8f
        );
    }
}
