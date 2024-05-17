using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class CollectableControl : MonoBehaviour
{
    public static int coinCount;
    public GameObject coinCountDisplay;
    public GameObject coinEndDisplay;

    //achievements vars
    public GameObject achievementUI;
    public GameObject achievementEndUItext;

    public GameObject achievementEndUIsubtext;
    public static List<int> treballadordelmes_coins = new List<int> {  30,100,200,300,500,600,1000,1500,2000,3000 };
    private int treballadordelmes_coins_index = 0;
    //private static int maxTimePlayed;
    //private float maxTimeResting = 60;

    // private bool HighScoreAchieved = false;
    // private bool SurvivalAchieved = false;
    //private bool RestfulAchieved = false;

    //audio mixer
    public AudioMixer audioMixer;
    private string exposedParameter;
    private float duration;
    private float targetVolume;
    public AudioSource highScoreSFX;


    void Start()
    {
        coinCount = 0;
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount;
        coinEndDisplay.GetComponent<Text>().text = "" + coinCount;
        achievementUI.SetActive(false);
    }
    void Update()
    {
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount;
        coinEndDisplay.GetComponent<Text>().text = "" + coinCount;

        if (treballadordelmes_coins_index < treballadordelmes_coins.Count)
        {
            if (coinCount >= treballadordelmes_coins[treballadordelmes_coins_index])
            {
                achievementEndUItext.GetComponent<Text>().text = "¡TREBALLADORA DEL MES!";
                achievementEndUIsubtext.GetComponent<Text>().text = "Has recolectat "+treballadordelmes_coins[treballadordelmes_coins_index].ToString() + " monedes!";
                achievementUI.SetActive(true);
                highScoreSFX.Play();
                StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 0.5f, targetVolume = 0.25f));
                StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 0.5f, targetVolume = 0.25f));
                treballadordelmes_coins_index += 1;
                //StartCoroutine(hideachievement());
            }
        }
        /*
         if (coinCount >= maxScore && !HighScoreAchieved)
        {
            highScoreSFX.Play();
            highScorePopUp.SetActive(true);
            HighScoreAchieved = true;
            //StartCoroutine FadeOutFrame;

            //record new high score!
        }
        */
    }

    IEnumerator hideachievement()
    {
        yield return new WaitForSeconds(2);
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 2, targetVolume = 1));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 2, targetVolume = 1));
        yield return new WaitForSeconds(3);
        achievementUI.SetActive(false);
    }
}


