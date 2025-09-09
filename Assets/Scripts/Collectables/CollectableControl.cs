using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class CollectableControl : MonoBehaviour, IResettable
{
    public GameObject player;
    public GameObject levelControl;
    public static int coinCount;
    public GameObject coinCountDisplay;
    public bool savingPlayerPrefences = true;  //if false, high scores are reset every day

    //achievements vars
    public GameObject achievementUI;
    public GameObject achievementEndUItext;
    public GameObject achievementEndUIsubtext;
    public static List<int> treballadordelmes_coins = new List<int> { 30, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1200 };
    private int treballadordelmes_coins_index = 0;
    public int highScore;
    public static bool highScoreAchieved = false;
    public bool firstAchievementMet = false;

    //time vars
    private float elapsedTime = 0f;
    private List<float> seconds_to_elapse = new List<float> { 60f, 120f, 180f, 240f, 360f, 420f, 520f };
    private int seconds_to_elapse_index = 0;
    public int ConvertSecondsToMinutes(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        return minutes;
    }

    //list of compliments
    private List<string> compliments = new List<string> { "DEL DIA", "DEL MES", "DE L'ANY", "TOTAL", "DEMENT", "MÀQUINA", "DEL SEGLE", "BRUTAL", "ESVERADA", "INSACIABLE", "MILEURISTA", "MODÈLICA" };

    // store the last achievement text
    public static string lastAchievementText = "";

    //store the high score message
    public static string highScoreText = "";

    //list of time compliments
    private List<string> timeCompliments = new List<string> { "INCANSABLE!", "INSACIABLE!", "IRREFRENABLE!", "NO POTS PARAR!", "EL TEMPS ÉS OR", "NO HI HA FINAL", "MORIRÀS TREBALLANT" };

    public bool achievementShown = false; //used to prevent collisions between score and time achievements
    public bool runtimeHighScoreTriggered = false;

    //audio
    public AudioMixer audioMixer;
    public AudioSource highScoreSFX;
    public AudioSource highSpeedSFX;
    private PlayerMove playerMove;

    void Start()
    {
        coinCount = 0;
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount;
        achievementUI.SetActive(false);
        lastAchievementText = "";
        highScoreText = "";

        // Load saved high score
        SessionData.LoadHighScore(savingPlayerPrefences);
        highScore = SessionData.sessionHighScore;

        playerMove = player.GetComponent<PlayerMove>();
    }

    public void HandlePlayerDeath()
    {
        Debug.Log("Collectable Control: Handling Player Death");

        if (coinCount > highScore)
        {
            highScore = coinCount;
            highScoreAchieved = true;
            SessionData.sessionHighScore = highScore;
            if (savingPlayerPrefences)
                SessionData.UpdateHighScore(highScore, savingPlayerPrefences);
            highScoreText = "NOU RÈCORD!";
            Debug.Log("New high score saved: " + highScore);
        }
        else if (coinCount <= highScore)
        {
            highScoreText = "ÚLTIM RECORD: " + highScore + " monedes";
            Debug.Log("Under last score");
        }
        else
        {
            highScoreText = "";
            Debug.Log("No new score.");
        }
    }

    void Update()
    {
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount;

        if (PlayerMove.startedrunning && !PlayerMove.isDead)
        {
            elapsedTime += Time.deltaTime;

            // Coin achievements
            if (treballadordelmes_coins_index < treballadordelmes_coins.Count)
            {
                if (coinCount == treballadordelmes_coins[treballadordelmes_coins_index])
                {
                  if (1==1){
                        string compliment = compliments[treballadordelmes_coins_index];
                        lastAchievementText = "TREBALLADORA " + compliment + "!";
                        achievementEndUItext.GetComponent<Text>().text = lastAchievementText;
                        achievementEndUIsubtext.GetComponent<Text>().text = "Has recol·lectat " + treballadordelmes_coins[treballadordelmes_coins_index].ToString() + " monedes!";
                        achievementUI.SetActive(true);
                        achievementShown = true;
                        highScoreSFX.Play();
                        dimVolumes();
                        lifeUp();
                        treballadordelmes_coins_index += 1;
                        StartCoroutine(hideachievement());
                    }
                }
        }
            // Time achievements
                if (seconds_to_elapse_index < seconds_to_elapse.Count)
                {
                    if (elapsedTime > seconds_to_elapse[seconds_to_elapse_index])
                    {
                if (!achievementShown)
                {
                    int elapsedMinutes = ConvertSecondsToMinutes(elapsedTime);

                    string timeCompliment = timeCompliments[seconds_to_elapse_index];
                    achievementEndUItext.GetComponent<Text>().text = timeCompliment;
                    achievementEndUIsubtext.GetComponent<Text>().text = "Has sobreviscut " + elapsedMinutes.ToString() + " minuts!";
                    achievementUI.SetActive(true);
                    achievementShown = true;
                    highSpeedSFX.Play();
                    dimVolumes();
                    lifeUp();
                    seconds_to_elapse_index += 1;
                        if (seconds_to_elapse_index == 1)
                        {
                            firstAchievementMet = true;
                            levelControl.GetComponent<GenerateSandstorm>().enabled = true;
                            levelControl.GetComponent<GenerateSandstorm>().StartSandstormGeneration();
                        }
                        StartCoroutine(hideachievement());
                }
                }
            }

            // High Score achieved
            if (!runtimeHighScoreTriggered && coinCount > highScore)
            {
                runtimeHighScoreTriggered = true;
                achievementEndUItext.GetComponent<Text>().text = "NOU RÈCORD!";
                achievementEndUIsubtext.GetComponent<Text>().text = "No et rendeixis!";
                achievementUI.SetActive(true);
                achievementShown = true;
                highScoreSFX.Play();
                highSpeedSFX.Play();
                dimVolumes();
                lifeUp();
                StartCoroutine(hideachievement());
            }
        }

        // Clear High Scores pressing R
        if (Input.GetKeyDown(KeyCode.R))
        {
            SessionData.ClearHighScore();
            highScore = 0;
            Debug.Log("High score reset.");
        }
    }

    void dimVolumes()
    {
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 0.5f, 0.15f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 0.5f, 0.25f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 0.5f, 0.25f));
    }

    void lifeUp()
    {
         playerMove.AddHeart();
    }

    IEnumerator hideachievement()
    {
        yield return new WaitForSeconds(2);
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 2, 0.75f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 2, 1f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 2, 1f));
        yield return new WaitForSeconds(3);
        achievementUI.SetActive(false);
        achievementShown = false;
    }

    public void ResetState()
    {
        Debug.Log("CollectableControl reset");
        coinCount = 0;
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount;
        lastAchievementText = "";
        highScoreText = "";

        // Reset achievements
        treballadordelmes_coins_index = 0;
        seconds_to_elapse_index = 0;
        elapsedTime = 0f;
        achievementShown = false;
        runtimeHighScoreTriggered = false;
        firstAchievementMet = false;
        highScoreAchieved = false;

        // Reset UI state
        achievementUI.SetActive(false);
        achievementEndUItext.GetComponent<Text>().text = "";
        achievementEndUIsubtext.GetComponent<Text>().text = "";

        // Reload saved high score
        SessionData.LoadHighScore(savingPlayerPrefences);
        highScore = SessionData.sessionHighScore;

        //playerMove = player.GetComponent<PlayerMove>();
    }

}