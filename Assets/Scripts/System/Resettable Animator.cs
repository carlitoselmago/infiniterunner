using UnityEngine;

public class ResettableAnimator : MonoBehaviour, IResettable
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ResetState()
    {
        if (animator == null) return;

        // Rewind to first frame of default state
        animator.Rebind();
        //animator.Update(0f);

        // If you want it to auto-play again:
        //animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
    }
}
