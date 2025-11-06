using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EndRunSequence : MonoBehaviour
{
    public PlayerMove player;
    public GameObject endCoinCount;
    public GameObject endScreen;
    public GameObject fadeOut;
    public GameObject fadeIn;
    public GameObject gameOverText;
    public GameObject highScoreDisplay;
    public GameObject levelControl;
    public GameObject highScoreCelebration;
    public AudioMixer audioMixer;
    public AudioSource gameOverFX;
    public GameObject startSection;

    void OnEnable()
    {
        ShowGameOverText();
        endScreen.SetActive(true);
        gameOverText.SetActive(true);
        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        // Fade out audio and stop sandstorm generation
        yield return new WaitForSeconds(0.33f);
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 1.5f, 0));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 1.5f, 0));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 1.5f, 0));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSandstorm", 1.5f, 0f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeModern", 1.5f, 0f));
        levelControl.GetComponent<GenerateSandstorm>().enabled = false;

        yield return new WaitForSeconds(1);

        // Show normal Game Over or High Score celebration
        if (CollectableControl.highScoreAchieved && highScoreCelebration != null)
                highScoreCelebration.SetActive(true);
        else
            gameOverFX.Play();

        endCoinCount.GetComponent<Text>().text = "Has recollit " + CollectableControl.coinCount + " monedes. \n" + CollectableControl.lastAchievementText;
        highScoreDisplay.GetComponent<Text>().text = CollectableControl.highScoreText;
        endCoinCount.SetActive(true);
        highScoreDisplay.SetActive(true);

        if (CollectableControl.highScoreAchieved)
            yield return new WaitForSeconds(5f);
        fadeOut.SetActive(true);

        // Animate UI
        if (CollectableControl.highScoreAchieved)
            yield return new WaitForSeconds(2f);
        else
            yield return new WaitForSeconds(2f);

        yield return new WaitForSeconds(2f);
        startSection.SetActive(true);
        ResetGame();
    }

    IEnumerator PopTextAnimation(Text txt, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        gameOverText.transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            gameOverText.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        gameOverText.transform.localScale = endScale;
    }

    public void ShowGameOverText()
    {
        Text txt = gameOverText.GetComponent<Text>();
        gameOverText.transform.localScale = Vector3.zero;
        gameOverText.SetActive(true);

        if (CollectableControl.highScoreAchieved)
        {
            txt.text = "NOU RÈCORD!";
            StartCoroutine(PopTextAnimation(txt, 0.65f));
        }
        else
        {
            txt.text = "GAME OVER";
            StartCoroutine(PopTextAnimation(txt, 2f));
        }
    }

    private void ResetGame()
    {
        Debug.Log("Requesting Reset");
        // Reset all IResettable scripts in the scene
        foreach (var resettable in FindObjectsOfType<MonoBehaviour>().OfType<IResettable>())
            resettable.ResetState();

        // Re-enable gameplay systems
        player.enabled = true;

        // Hide end screen UI
        endScreen.SetActive(false);
        endCoinCount.SetActive(false);
        highScoreDisplay.SetActive(false);
        StartCoroutine(ReloadSequence());
        fadeOut.SetActive(false);
        ResetAnimators(gameOverText, endCoinCount, highScoreDisplay);

        // Restore audio volumes
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 1f, 0.75f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 1f, 1f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 1f, 1f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSandstorm", 1f, 1f));
    }

    private void ResetAnimators(params GameObject[] objects)
    {
        foreach (var obj in objects)
        {
            var animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
                animator.enabled = false;
            }
        }
    }

    IEnumerator ReloadSequence()
    {
        fadeIn.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        fadeIn.SetActive(false);
    }
}