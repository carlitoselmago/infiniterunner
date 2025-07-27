using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;

public class GenerateSandstorm : MonoBehaviour
{

    public GameObject sandstorm;
    private Volume sandstormVolume;
    private ParticleSystem sandstormParticles;
    private Material sandstormMaterial; // Particle material
    public GameObject particles;
    public GameObject sandstormText;

    //audio mixer
    public AudioMixer audioMixer;
    private string exposedParameter;
    private float duration;
    private float targetVolume;
    public AudioSource sandstormFX;
    public AudioSource endStormSFX;

    [Header("Settings")]
    public float fadeDuration = 5f;
    public float maxFogDensity = 0.05f;
    public float minFogDensity = 0f;
    //public Color fogColor = new Color(0.9f, 0.8f, 0.6f);
    public float maxParticleAlpha = 1f;
    public float chance = 0.5f;
    public bool generatingSandstorm = false;

    private void Awake()
    {
        sandstormVolume = sandstorm.GetComponent<Volume>();
        sandstormParticles = particles.GetComponent<ParticleSystem>();
        sandstormMaterial = particles.GetComponent<ParticleSystemRenderer>().material;

        //sandstormVolume.weight = 0f;
        //sandstormMaterial.SetFloat("_GlobalAlpha", 0f); // Start transparent
    }

    private void OnEnable()
    {
        //RenderSettings.fog = true;
        //RenderSettings.fogColor = fogColor;
        //RenderSettings.fogMode = FogMode.ExponentialSquared;
        //RenderSettings.fogDensity = minFogDensity;
    }

    public void StartSandstormGeneration()
    {
        StartCoroutine(GenerateTheSandstorm(2f));
        //Debug.Log("Sandstorm Script Enabled");
    }

    IEnumerator GenerateTheSandstorm(float prewait)
    {
        float randomWait = Random.Range(0f, 40f);

        yield return new WaitForSeconds(prewait + randomWait); // delay before attempting to generate sandstorm
        if (Random.value > chance)
        {
            //Debug.Log("Generating Sandstorm");
            generatingSandstorm = true;

            sandstormParticles.Play();
            particles.SetActive(true);
            StartCoroutine(FadeFog(minFogDensity, maxFogDensity, fadeDuration));
            StartCoroutine(FadeVolumeAndParticles(0f, 1f, fadeDuration));
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeSandstorm", duration = fadeDuration, targetVolume = 1.2f));
            sandstormFX.Play();
            yield return new WaitForSeconds(fadeDuration / 2);
            sandstormText.SetActive(true);
            yield return new WaitForSeconds(3);
            sandstormText.SetActive(false);

            yield return new WaitForSeconds(Random.Range(10f, Random.Range(30f, 60f)));

            //stop sandstorm
            StartCoroutine(FadeFog(maxFogDensity, minFogDensity, fadeDuration));
            StartCoroutine(FadeVolumeAndParticles(1f, 0f, fadeDuration));
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeSandstorm", duration = fadeDuration, targetVolume = 0f));
            yield return new WaitForSeconds(1);
            endStormSFX.Play();
            yield return new WaitForSeconds(fadeDuration);
            sandstormFX.Stop();
            //Debug.Log("Sandstorm is over.");
            generatingSandstorm = false;

            StartCoroutine(GenerateTheSandstorm(30f));
        }
        else
        {
            //Debug.Log("Skipped Sandstrom based on chance");
            StartCoroutine(GenerateTheSandstorm(10f));
        }
    }

    IEnumerator FadeFog(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            RenderSettings.fogDensity = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        RenderSettings.fogDensity = to;
    }

    IEnumerator FadeVolumeAndParticles(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            sandstormVolume.weight = Mathf.Lerp(from, to, elapsed / duration);
            float alpha = Mathf.Lerp(from, to, elapsed / (duration/2));
            sandstormMaterial.SetFloat("_GlobalAlpha", alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        sandstormVolume.weight = to;
        sandstormMaterial.SetFloat("_GlobalAlpha", to);
        if (to == 0f)
        {
            sandstormParticles.Stop();
        }
    }

}