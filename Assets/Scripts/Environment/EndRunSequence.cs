using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndRunSequence : MonoBehaviour
{
    public GameObject liveCoins;
    public GameObject endScreen;
    public GameObject fadeOut;
    public GameObject gameOverText;
    void Start()
    {
        StartCoroutine(EndSequence());
    }

IEnumerator EndSequence()
    {
        yield return new WaitForSeconds(1);
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
