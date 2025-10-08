using UnityEngine;

public class PlayRandomHitSound : MonoBehaviour
{
    [Header("Collision Sounds")]
    public AudioSource audioSource;
    public AudioClip[] hitClips;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("car"))
        {
            if (hitClips.Length == 0 || audioSource == null) return;
            int index = Random.Range(0, hitClips.Length);
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.PlayOneShot(hitClips[index]);
        }
    }
}