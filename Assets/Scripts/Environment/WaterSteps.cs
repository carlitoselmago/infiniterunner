using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaterSteps : MonoBehaviour
{
    [Header("Footsteps Audio")]
    public AudioClip footstepsClip;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = footstepsClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}