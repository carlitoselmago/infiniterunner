using System.Collections;
using System.Linq;
using UnityEngine;
//using UnityEngine.SceneManagement;
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
    public AudioMixer audioMixer;
    public AudioSource gameOverFX;

    void OnEnable()
    {
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
        levelControl.GetComponent<GenerateSandstorm>().enabled = false;

        // Show end screen
        endScreen.SetActive(true);
        yield return new WaitForSeconds(1);
        gameOverFX.Play();
        endCoinCount.GetComponent<Text>().text = "Has recollit " + CollectableControl.coinCount + " monedes. \n" + CollectableControl.lastAchievementText;
        highScoreDisplay.GetComponent<Text>().text = CollectableControl.highScoreText;
        endCoinCount.SetActive(true);
        highScoreDisplay.SetActive(true);
        fadeOut.SetActive(true);

        // Animate UI
        yield return new WaitForSeconds(2);
        gameOverText.GetComponent<Animator>().enabled = true;
        gameOverText.GetComponent<Animator>().Play("FadeOutText");
        endCoinCount.GetComponent<Animator>().enabled = true;
        endCoinCount.GetComponent<Animator>().Play("FadeOutText");
        highScoreDisplay.GetComponent<Animator>().enabled = true;
        highScoreDisplay.GetComponent<Animator>().Play("FadeOutText");

        //yield return new WaitForSeconds(2.5f);
        yield return new WaitForSeconds(2f);
        ResetGame();
        //SceneManager.LoadScene(0);
    }

    private void ResetGame()
    {
        Debug.Log("Requesting Reset");
        // Reset all IResettable scripts in the scene
        foreach (var resettable in FindObjectsOfType<MonoBehaviour>().OfType<IResettable>())
            resettable.ResetState();

        // Re-enable gameplay systems
        player.enabled = true;
        //levelControl.GetComponent<GenerateSandstorm>().enabled = true;


        // Hide end screen UI
        endScreen.SetActive(false);
        endCoinCount.SetActive(false);
        highScoreDisplay.SetActive(false);
        StartCoroutine(ReloadSequence());
        fadeOut.SetActive(false);
        gameOverText.GetComponent<Animator>().enabled = false;
        endCoinCount.GetComponent<Animator>().enabled = false;
        highScoreDisplay.GetComponent<Animator>().enabled = false;

        // Restore audio volumes
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 1f, 0.75f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 1f, 1f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 1f, 1f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSandstorm", 1f, 1f));
    }

    IEnumerator ReloadSequence()
    {
        fadeIn.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        fadeIn.SetActive(false);
    }
}