using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleSystemController : MonoBehaviour
{
    public ParticleSystem cloudParticleSystem; // Reference to the particle system
    public float activationDistance = 50f; // Distance to start activation
    public float fadeDuration = 2f; // Duration to fade in the particle system

    private GameObject player;
    private bool isActivated = false;

    void Start()
    {
        // Automatically find the player GameObject
        player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found. Make sure there is a GameObject named 'Player' in the scene.");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance <= activationDistance && !isActivated)
        {
            StartCoroutine(FadeInParticleSystem());
            isActivated = true;
        }
    }

    private IEnumerator FadeInParticleSystem()
    {
        ParticleSystem.EmissionModule emission = cloudParticleSystem.emission;
        float initialRate = emission.rateOverTime.constant;
        emission.rateOverTime = 0;

        cloudParticleSystem.Play();

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            emission.rateOverTime = Mathf.Lerp(0, initialRate, elapsedTime / fadeDuration);
            yield return null;
        }

        emission.rateOverTime = initialRate;
    }
}
