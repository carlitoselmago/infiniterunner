using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour
{
    public GameObject targetWithAudio;
    private AudioSource audioSource;

    void Start()
    {
        if (targetWithAudio != null)
        {
            audioSource = targetWithAudio.GetComponent<AudioSource>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}