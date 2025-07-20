using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EndRunSequence : MonoBehaviour
{
    public GameObject endCoinCount;
    public GameObject endScreen;
    public GameObject fadeOut;
    public GameObject gameOverText;
    public GameObject highScoreDisplay;
    public AudioMixer audioMixer;
    public AudioSource gameOverFX;
    private string exposedParameter;
    private float duration;
    private float targetVolume;

    void Start()
    {
        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        yield return new WaitForSeconds(0.33f);
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 1.5f, targetVolume = 0));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 1.5f, targetVolume = 0));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeSFX", duration = 1.5f, targetVolume = 0));
        endScreen.SetActive(true);
        yield return new WaitForSeconds(1);
        gameOverFX.Play();
        endCoinCount.GetComponent<Text>().text = "Has recollit " + CollectableControl.coinCount + " monedes. \n" + CollectableControl.lastAchievementText;
        highScoreDisplay.GetComponent<Text>().text = CollectableControl.highScoreText;
        endCoinCount.SetActive(true);
        highScoreDisplay.SetActive(true);
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(2);
        gameOverText.GetComponent<Animator>().enabled = true;
        gameOverText.GetComponent<Animator>().Play("FadeOutText");
        endCoinCount.GetComponent<Animator>().enabled = true;
        endCoinCount.GetComponent<Animator>().Play("FadeOutText");
        highScoreDisplay.GetComponent<Animator>().enabled = true;
        highScoreDisplay.GetComponent<Animator>().Play("FadeOutText");

        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene(0);
    }
}