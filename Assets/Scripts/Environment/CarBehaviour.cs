using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    private Animator animator;
    public string animationTriggerName = "carmoving";  // Name of the trigger to activate the animation

    private float timer;
    private float interval;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on this GameObject.");
            enabled = false;  // Disable this script if no Animator component is found
            return;
        }
        SetRandomInterval();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            ActivateAnimator();
            SetRandomInterval();
            timer = 0f;
        }
    }

    void SetRandomInterval()
    {
        interval = Random.Range(0f, 4f);
    }

    void ActivateAnimator()
    {
        if (animator != null)
        {
            animator.SetTrigger(animationTriggerName);
        }
    }
}