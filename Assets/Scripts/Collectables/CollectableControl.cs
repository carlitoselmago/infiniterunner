using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class CollectableControl : MonoBehaviour
{
    public static int coinCount;
    public GameObject coinCountDisplay;

    //achievements vars
    public GameObject achievementUI;
    public GameObject achievementEndUItext;
    public GameObject achievementEndUIsubtext;
    public static List<int> treballadordelmes_coins = new List<int> { 30, 100, 200, 300, 400, 500, 600, 700, 800, 900 };
    private int treballadordelmes_coins_index = 0;

    //list of compliments
    private List<string> compliments = new List<string> { "DEL DIA", "DEL MES", "DE L'ANY", "TOTAL", "DEMENT", "MÀQUINA", "COMPULSIVA", "BRUTAL", "ESVERADA", "INSACIABLE" };

    // store the last achievement text
    public static string lastAchievementText = "";

    //audio mixer
    public AudioMixer audioMixer;
    private string exposedParameter;
    private float duration;
    private float targetVolume;
    private bool accelerating = true;
    public static bool maxSpeedIsReached = false;
    public AudioSource highScoreSFX;
    public AudioSource highSpeedSFX;

    void Start()
    {
        coinCount = 0;
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount;
        achievementUI.SetActive(false);
        lastAchievementText = "";
    }

    void Update()
    {
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount;

        if (treballadordelmes_coins_index < treballadordelmes_coins.Count)
        {
            if (coinCount >= treballadordelmes_coins[treballadordelmes_coins_index])
            {
                string compliment = compliments[treballadordelmes_coins_index];
                lastAchievementText = "TREBALLADORA " + compliment + "!";
                achievementEndUItext.GetComponent<Text>().text = lastAchievementText;
                achievementEndUIsubtext.GetComponent<Text>().text = "Has recol·lectat " + treballadordelmes_coins[treballadordelmes_coins_index].ToString() + " monedes!";
                achievementUI.SetActive(true);
                highScoreSFX.Play();
                StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 0.5f, targetVolume = 0.25f));
                StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 0.5f, targetVolume = 0.25f));
                treballadordelmes_coins_index += 1;
                StartCoroutine(hideachievement());
            }
        }

        //acceleration
        if (accelerating)
        {
            if (!maxSpeedIsReached)
            {
                achievementEndUItext.GetComponent<Text>().text = "MÀXIMA VELOCITAT!";
                achievementUI.SetActive(true);
                highSpeedSFX.Play();
                StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 0.5f, targetVolume = 0.25f));
                StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 0.5f, targetVolume = 0.25f));
                StartCoroutine(hideachievement());
                accelerating = false;
            }
        } else
        {
            Debug.Log("Acceleration complete");
        }
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
