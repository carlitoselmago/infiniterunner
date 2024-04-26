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
    private string exposedParameter = "volumeBGM";
    private float duration;
    private float targetVolume;


    void Start()
    {
        StartCoroutine(EndSequence());
    }

IEnumerator EndSequence()
    {
        yield return new WaitForSeconds(1);
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter, duration = 3, targetVolume = 0));
        endScreen.SetActive(true);
        yield return new WaitForSeconds(1);
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(2);
        gameOverText.GetComponent<Animator>().enabled = true;
        gameOverText.GetComponent<Animator>().Play("FadeOutText");
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }

}