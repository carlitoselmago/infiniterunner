using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour
{
    public GameObject targetWithAudio;
    private AudioSource audioSource;

    void Start()
    {
    audioSource = targetWithAudio.GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audioSource.Play();
        }
    }
}