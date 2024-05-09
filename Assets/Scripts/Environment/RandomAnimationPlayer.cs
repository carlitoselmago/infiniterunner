using UnityEngine;

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
        // Increment timer
        timer += Time.deltaTime;

        // Check if it's time to play a new animation
        if (timer >= interval)
        {
            // Randomly select an animation
            int randomIndex = Random.Range(0, animationNames.Length);
            string randomAnimation = animationNames[randomIndex];

            // Play the selected animation
            animator.Play(randomAnimation);

            // Reset timer
            timer = 0f;
        }
    }
}
