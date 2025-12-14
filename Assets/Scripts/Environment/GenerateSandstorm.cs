using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;

public class GenerateSandstorm : MonoBehaviour, IResettable
{

    public GameObject sandstorm;
    private Volume sandstormVolume;
    private ParticleSystem sandstormParticles;
    private Material sandstormMaterial; // Particle material
    public GameObject particles;
    public GameObject sandstormText;

    //audio mixer
    public AudioMixer audioMixer;
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
    private bool sandstormGeneratorEnabled = false;
    private bool triggered = false;
    private Coroutine stormCoroutine;

    private void Awake()
    {
        sandstormVolume = sandstorm.GetComponent<Volume>();
        sandstormParticles = particles.GetComponent<ParticleSystem>();
        sandstormMaterial = particles.GetComponent<ParticleSystemRenderer>().material;
        sandstormVolume.weight = 0f;
        //sandstormMaterial.SetFloat("_GlobalAlpha", 0f); // Start transparent
    }

    private void OnEnable()
    {
        //RenderSettings.fog = true;
        //RenderSettings.fogColor = fogColor;
        //RenderSettings.fogMode = FogMode.ExponentialSquared;
        //RenderSettings.fogDensity = minFogDensity;
    }

    void Update()
    {
        if (!generatingSandstorm) return;

        if ((MineData.isInTheMine || MusicEventController.isInMidiLevel) && sandstormGeneratorEnabled)
        {
            StopAllCoroutines(); // stop any active fade in/out coroutines
            StopTheSandstorm();
            StartCoroutine(FadeOutAndStopAudio());
            generatingSandstorm = false;
            PlayerMove.isInTheSandstorm = false;
        }

        if (PlayerMove.isUnderwater && !triggered)
        {
            triggered = true;
            StopAllCoroutines();
            StartCoroutine(FadeFog(maxFogDensity, minFogDensity, 0.3f));
            StartCoroutine(FadeVolumeAndParticles(1f, 0f, 0.3f));
        }

        if (PlayerMove.isDead)
        {
            StopAllCoroutines();
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSandstorm", 2f, 0f));
            sandstormFX.Stop();
        }
    }

    public void StartSandstormGeneration()
    {
        sandstormGeneratorEnabled = true;
        if (stormCoroutine != null) StopCoroutine(stormCoroutine);         // Stop any old one before starting a new one
        stormCoroutine = StartCoroutine(GenerateTheSandstorm(2f));
        Debug.Log("Sandstorm Script Enabled");
    }

    IEnumerator GenerateTheSandstorm(float prewait)
    {
        yield return new WaitForSeconds(prewait);

        while (sandstormGeneratorEnabled)
        {
            if (MineData.isInTheMine || PlayerMove.isOnModernTimes) yield break;
            float randomWait = Random.Range(0f, 40f);
            if (!sandstormGeneratorEnabled)
                yield return new WaitForSeconds(randomWait); // delay before attempting to generate sandstorm

            if (Random.value > chance)
            {
                Debug.Log("Generating Sandstorm...");
                generatingSandstorm = true;
                PlayerMove.isInTheSandstorm = true;
                sandstormParticles.Play();
                particles.SetActive(true);
                StartCoroutine(FadeFog(minFogDensity, maxFogDensity, fadeDuration));
                StartCoroutine(FadeVolumeAndParticles(0f, 1f, fadeDuration));
                StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSandstorm", fadeDuration, 1.2f));
                sandstormFX.Play();
                yield return new WaitForSeconds(fadeDuration / 2);
                sandstormText.SetActive(true);
                yield return new WaitForSeconds(3);
                sandstormText.SetActive(false);

                yield return new WaitForSeconds(Random.Range(10f, Random.Range(30f, 60f)));

                //stop sandstorm
                StopTheSandstorm();
                yield return new WaitForSeconds(1);
                if (!PlayerMove.isOnModernTimes)
                    endStormSFX.Play();
                yield return new WaitForSeconds(fadeDuration);
                sandstormFX.Stop();
                generatingSandstorm = false;
                PlayerMove.isInTheSandstorm = false;
                yield return new WaitForSeconds(30f);
            }
            else
            {
                Debug.Log("Skipped Sandstrom based on chance");
                yield return new WaitForSeconds(10f);
            }
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
            float alpha = Mathf.Lerp(from, to, elapsed / (duration / 2));
            sandstormMaterial.SetFloat("_GlobalAlpha", alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        sandstormVolume.weight = to;
        sandstormMaterial.SetFloat("_GlobalAlpha", to);
        if (to == 0f)
            sandstormParticles.Stop();

    }

    public void StopTheSandstorm()
    {
        if (!generatingSandstorm) return;
        sandstormText.SetActive(false);
        StartCoroutine(FadeFog(maxFogDensity, minFogDensity, fadeDuration));
        StartCoroutine(FadeVolumeAndParticles(1f, 0f, fadeDuration));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSandstorm", fadeDuration, 0f));
        Debug.Log("StopTheSandstorm");
    }

    private IEnumerator FadeOutAndStopAudio()
    {
        // Fade the mixer group volume down to 0 smoothly
        yield return StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSandstorm", 2f, 0f));

        // Stop both audio sources cleanly
        sandstormFX.Stop();
        endStormSFX.Stop();
    }

    private void OnDisable()
    {
        if (stormCoroutine != null)
        {
            StopCoroutine(stormCoroutine);
            stormCoroutine = null;
        }

        generatingSandstorm = false;
        PlayerMove.isInTheSandstorm = false;
        sandstormGeneratorEnabled = false;

        StartCoroutine(FadeOutAndStopAudio());
    }


    public void ResetState()
    {
        if (stormCoroutine != null)
        {
            StopCoroutine(stormCoroutine);
            stormCoroutine = null;
        }

        sandstormGeneratorEnabled = false;   // disable script until called again
        generatingSandstorm = false;
        triggered = false;
        StopTheSandstorm();
        StartCoroutine(FadeOutAndStopAudio());

        RenderSettings.fogDensity = 0;
        sandstormMaterial.SetFloat("_GlobalAlpha", 0f);
        sandstormVolume.weight = 0;
        sandstormParticles.Stop();
        StopCoroutine(GenerateTheSandstorm(0));
    }

}