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

    //time vars
    private float elapsedTime = 0f;
    private List<float> seconds_to_elapse = new List<float> { 60f, 120f, 180f, 240f, 360f, 420f };
    private int seconds_to_elapse_index = 0;
    public int ConvertSecondsToMinutes(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        return minutes;
    }

    //list of compliments
    private List<string> compliments = new List<string> { "DEL DIA", "DEL MES", "DE L'ANY", "TOTAL", "DEMENT", "MÀQUINA", "COMPULSIVA", "BRUTAL", "ESVERADA", "INSACIABLE" };

    // store the last achievement text
    public static string lastAchievementText = "";

    //list of time compliments
    private List<string> timeCompliments = new List<string> { "INCANSABLE!", "INSACIABLE!", "IRREFRENABLE!", "NO POTS PARAR!", "EL TEMPS ÉS OR", "NO HI HA FINAL" };

    private bool achievementShown = false; //used to prevent collisions between score and time achievements

    //audio mixer
    public AudioMixer audioMixer;
    private string exposedParameter;
    private float duration;
    private float targetVolume;
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
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount; //alternative for time tracking: elapsedTime

        if (PlayerMove.startedrunning)
        {
            elapsedTime += Time.deltaTime;

            if (treballadordelmes_coins_index < treballadordelmes_coins.Count)
            {
                if (coinCount >= treballadordelmes_coins[treballadordelmes_coins_index])
                {
                    if (achievementShown == false)
                    {
                        string compliment = compliments[treballadordelmes_coins_index];
                        lastAchievementText = "TREBALLADORA " + compliment + "!";
                        achievementEndUItext.GetComponent<Text>().text = lastAchievementText;
                        achievementEndUIsubtext.GetComponent<Text>().text = "Has recol·lectat " + treballadordelmes_coins[treballadordelmes_coins_index].ToString() + " monedes!";
                        achievementUI.SetActive(true);
                        achievementShown = true;
                        highScoreSFX.Play();
                        dimVolumes();
                        treballadordelmes_coins_index += 1;
                        StartCoroutine(hideachievement());
                    }
                    else
                    {
                        Debug.LogError("Skipped High Score Achievement!");
                    }
                }
            
        }
            
                if (seconds_to_elapse_index < seconds_to_elapse.Count)
                {
                    if (elapsedTime > seconds_to_elapse[seconds_to_elapse_index])
                    {
                if (achievementShown == false)
                {
                    int elapsedMinutes = ConvertSecondsToMinutes(elapsedTime);

                    string timeCompliment = timeCompliments[seconds_to_elapse_index];
                    achievementEndUItext.GetComponent<Text>().text = timeCompliment;
                    achievementEndUIsubtext.GetComponent<Text>().text = "Has sobreviscut " + elapsedMinutes.ToString() + " minuts!";
                    achievementUI.SetActive(true);
                    achievementShown = true;
                    highSpeedSFX.Play();
                    dimVolumes();
                    seconds_to_elapse_index += 1;
                    StartCoroutine(hideachievement());
                }
                else
                {
                    Debug.LogError("Skipped Time Achievement!");
                }

                }
            }

        }

    }

    void dimVolumes()
    {
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 0.5f, targetVolume = 0.25f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 0.5f, targetVolume = 0.25f));
    }

    IEnumerator hideachievement()
    {
        yield return new WaitForSeconds(2);
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 2, targetVolume = 1));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 2, targetVolume = 1));
        yield return new WaitForSeconds(3);
        achievementUI.SetActive(false);
        achievementShown = false;
    }
}
