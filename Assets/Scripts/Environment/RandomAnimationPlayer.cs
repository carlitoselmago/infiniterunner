using UnityEngine;

// Animation player used for the falling cogs scene
public class RandomAnimationPlayer : MonoBehaviour
{
    public Animator animator;
    public string[] animationNames;
    private float timer = 0f;
    private float interval = 9f;

    void Start()
    {
        // Initialize the timer randomly to prevent synchronized animations at the start
        timer = Random.Range(0f, interval);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            int randomIndex = Random.Range(0, animationNames.Length);
            string randomAnimation = animationNames[randomIndex];
            animator.Play(randomAnimation);
            timer = 0f;
        }
    }
}