using UnityEngine;

public class ResettableAnimator : MonoBehaviour, IResettable
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ResetState()
    {
        if (animator == null) return;
        animator.Rebind();
    }
}
