using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class WaterTrigger : MonoBehaviour, IResettable
{
    public Volume triggeredVolume;
    public float transitionDuration = 0.3f;
    public GameObject splashSound;
    public CollectableControl collectableControl;
    public GameObject player;

    private Coroutine transitionCoroutine;
    private bool triggered = false;

    private void Start()
    {
        collectableControl = FindObjectOfType<CollectableControl>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            triggered = true;
            PlayerMove.isUnderwater = true;
            //Debug.Log("Underwater");
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

    public void ResetState()
    {
        triggeredVolume.gameObject.SetActive(false);
        splashSound.SetActive(false);
        triggered = false;
    }
}