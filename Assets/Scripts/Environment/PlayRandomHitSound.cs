using UnityEngine;

public class PlayRandomHitSound : MonoBehaviour
{
    [Header("Collision Sounds")]
    public AudioSource audioSource;
    public AudioClip[] hitClips;

    private float lastPlayTime;
    public float minInterval = 0.3f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1;
        audioSource.minDistance = 5;
        audioSource.maxDistance = 18;
    }
    /*
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("car") || other.CompareTag("obstacle"))
        {
            if (hitClips.Length == 0 || audioSource == null) return;
            int index = Random.Range(0, hitClips.Length);
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.PlayOneShot(hitClips[index]);
        }
    }*/

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude < 1.5f)
            return;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") &&
            !other.CompareTag("car") &&
            !other.CompareTag("obstacle"))
            return;

        if (Time.time - lastPlayTime < minInterval)
            return;

        if (hitClips.Length == 0 || audioSource == null)
            return;

        lastPlayTime = Time.time;

        int index = Random.Range(0, hitClips.Length);
        audioSource.pitch = Random.Range(0.8f, 1.1f);
        audioSource.PlayOneShot(hitClips[index]);
    }
}