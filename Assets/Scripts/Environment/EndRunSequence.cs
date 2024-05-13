using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class EndRunSequence : MonoBehaviour
{
    public GameObject liveCoins;
    public GameObject endScreen;
    public GameObject fadeOut;
    public GameObject gameOverText;
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
        yield return new WaitForSeconds(1);
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 1.5f, targetVolume = 0));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 1.5f, targetVolume = 0));
        endScreen.SetActive(true);
        yield return new WaitForSeconds(1);
        gameOverFX.Play();
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(2);
        gameOverText.GetComponent<Animator>().enabled = true;
        gameOverText.GetComponent<Animator>().Play("FadeOutText");
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(0);
    }

}