using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class WaterTrigger : MonoBehaviour
{
    public Volume triggeredVolume;
    public float transitionDuration = 0.3f;
    public GameObject splashSound;

    private Coroutine transitionCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Underwater");

            triggeredVolume.gameObject.SetActive(true);
            splashSound.SetActive(true);
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(FadeVolumeWeight(triggeredVolume, 0f, 1f, transitionDuration));
        }
    }

    private IEnumerator FadeVolumeWeight(Volume volume, float startWeight, float endWeight, float duration)
    {
        float elapsed = 0f;

        volume.weight = startWeight;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            volume.weight = Mathf.Lerp(startWeight, endWeight, elapsed / duration);
            yield return null;
        }

        volume.weight = endWeight;
    }
}