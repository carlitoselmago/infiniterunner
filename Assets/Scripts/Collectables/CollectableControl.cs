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
    public bool savingPlayerPrefences = true;

    // Achievement UI
    public GameObject achievementUI;
    public GameObject achievementEndUItext;
    public GameObject achievementEndUIsubtext;

    // Achievements data
    public static List<int> treballadordelmes_coins = new List<int> { 30, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1200 };
    private int treballadordelmes_coins_index = 0;
    public int highScore;
    public static bool highScoreAchieved = false;
    public static bool firstSkateboard = false;
    private bool firstSkateboardAchieved = false;
    public static bool firstForklift = false;
    private bool firstForkliftAchieved = false;
    public bool firstAchievementMet = false;

    // Time tracking
    private float elapsedTime = 0f;
    private List<float> seconds_to_elapse = new List<float> { 60f, 120f, 180f, 240f, 360f, 420f, 520f, 700f };
    private int seconds_to_elapse_index = 0;

    private List<string> compliments = new List<string> { "DEL DIA", "DEL MES", "DE L'ANY", "TOTAL", "DEMENT", "MÀQUINA", "DEL SEGLE", "BRUTAL", "ESVERADA", "BOJA", "MODEL", "DIVINA" };
    private List<string> timeCompliments = new List<string> { "INCANSABLE!", "INSACIABLE!", "IRREFRENABLE!", "NO POTS PARAR!", "EL TEMPS ÉS OR", "NO HI HA FINAL", "MORIRÀS TREBALLANT", "NO HI HA FUTUR" };

    // Achievement state
    public static string lastAchievementText = "";
    public static string highScoreText = "";
    public static bool achievementShown = false;
    public bool runtimeHighScoreTriggered = false;

    // Audio
    public AudioMixer audioMixer;
    public AudioSource highScoreSFX;
    public AudioSource highSpeedSFX;
    public AudioSource coinStreakSFX;
    public AudioSource coinFX;
    private PlayerMove playerMove;

    // Coin streak bonus
    public int streakStart = 6;        // Start counting glissando from this many coins
    public int streakMax = 10;         // Achievement triggers at this number
    public float streakWindow = 1f;    // Time window in seconds
    private Queue<float> recentCoinTimes = new Queue<float>();
    [SerializeField] CoinStreakUI streakUI;


    void Start()
    {
        coinCount = 0;
        coinCountDisplay.GetComponent<Text>().text = coinCount.ToString();
        achievementUI.SetActive(false);
        lastAchievementText = "";
        highScoreText = "";

        SessionData.LoadHighScore(savingPlayerPrefences);
        highScore = SessionData.sessionHighScore;

        playerMove = player.GetComponent<PlayerMove>();
    }

    public void HandlePlayerDeath()
    {
        if (coinCount > highScore)
        {
            highScore = coinCount;
            highScoreAchieved = true;
            SessionData.sessionHighScore = highScore;
            if (savingPlayerPrefences)
                SessionData.UpdateHighScore(highScore, savingPlayerPrefences);
            highScoreText = "";
        }
        else
        {
            highScoreText = "ÚLTIM RECORD: " + highScore + " monedes";
        }
    }

    void Update()
    {
        coinCountDisplay.GetComponent<Text>().text = coinCount.ToString();

        if (PlayerMove.startedrunning && !PlayerMove.isDead)
        {
            elapsedTime += Time.deltaTime;

            HandleCoinAchievements();
            HandleTimeAchievements();
            HandleHighScoreAchievement();
            HandleFirstForkliftAchievement();
            HandleFirstSkateboardAchievement();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SessionData.ClearHighScore();
            highScore = 0;
            Debug.Log("High score reset.");
        }
    }

    public void OnCoinCollected()
    {
        if (PlayerMove.isOnTheAir)
        {
            coinFX.pitch = 1f;
            coinFX.Play();
            return;
        }

        float now = Time.time;
        recentCoinTimes.Enqueue(now);

        // Remove coins outside the streak window
        while (recentCoinTimes.Count > 0 && now - recentCoinTimes.Peek() > streakWindow)
            recentCoinTimes.Dequeue();

        int streakCount = recentCoinTimes.Count;

        // --- Handle audio glissando ---
        if (streakCount >= streakStart && streakCount <= streakMax && coinFX != null)
        {
            float t = (float)(streakCount - streakStart) / (streakMax - streakStart);
            float pitch = 1f + Mathf.Pow(t, 1.5f) * 0.5f;
            coinFX.pitch = Mathf.Clamp(pitch, 1f, 1.5f);
        }
        else if (streakCount < streakStart && coinFX != null)
        {
            coinFX.pitch = 1f; // reset to normal
        }

        coinFX.Play();

        // --- Update visual streak UI ---
        if (streakUI != null)
            streakUI.UpdateDots(streakCount, streakStart, streakMax);

        // --- Streak bonus achievement ---
        if (streakCount >= streakMax)
        {
            playerMove.AddHeart();
            coinStreakSFX.Play();
            recentCoinTimes.Clear(); // reset streak
            coinFX.pitch = 1f;

            // Trigger UI celebration
            if (streakUI != null)
                streakUI.PlayAchievementFlash();

            StartCoroutine(hideachievement());
        }
    }

    void HandleCoinAchievements()
    {
        if (treballadordelmes_coins_index < treballadordelmes_coins.Count)
        {
            if (coinCount == treballadordelmes_coins[treballadordelmes_coins_index] && !achievementShown)
            {
                string compliment = compliments[treballadordelmes_coins_index];
                lastAchievementText = "TREBALLADORA " + compliment + "!";
                Debug.Log("New Achievement: " + compliment);
                achievementEndUItext.GetComponent<Text>().text = lastAchievementText;
                achievementEndUIsubtext.GetComponent<Text>().text = "Has recol·lectat " + treballadordelmes_coins[treballadordelmes_coins_index] + " monedes!";
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

    void HandleTimeAchievements()
    {
        if (seconds_to_elapse_index < seconds_to_elapse.Count)
        {
            if (elapsedTime > seconds_to_elapse[seconds_to_elapse_index] && !achievementShown)
            {
                int elapsedMinutes = Mathf.FloorToInt(elapsedTime / 60f);
                string timeCompliment = timeCompliments[seconds_to_elapse_index];
                achievementEndUItext.GetComponent<Text>().text = timeCompliment;
                Debug.Log("New Achievement: " + timeCompliment);
                achievementEndUIsubtext.GetComponent<Text>().text = "Has sobreviscut " + elapsedMinutes + " minuts!";
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

    void HandleHighScoreAchievement()
    {
        if (!runtimeHighScoreTriggered && coinCount > highScore)
        {
            runtimeHighScoreTriggered = true;
            achievementEndUItext.GetComponent<Text>().text = "NOU RÈCORD!";
            achievementEndUIsubtext.GetComponent<Text>().text = "No et rendeixis!";
            Debug.Log("New Achievement: Nou Rècord!");
            achievementUI.SetActive(true);
            achievementShown = true;
            highScoreSFX.Play();
            highSpeedSFX.Play();
            dimVolumes();
            lifeUp();
            StartCoroutine(hideachievement());
        }
    }

    void HandleFirstForkliftAchievement()
    {
        if (firstForklift && !firstForkliftAchieved)
        {
            firstForkliftAchieved = true;
            achievementEndUItext.GetComponent<Text>().text = "AL TORO!";
            achievementEndUIsubtext.GetComponent<Text>().text = "Has après a conduir!";
            Debug.Log("New Achievement: Al Toro!");
            achievementUI.SetActive(true);
            achievementShown = true;
            highSpeedSFX.Play();
            dimVolumes();
            lifeUp();
            StartCoroutine(hideachievement());
        }
    }

    void HandleFirstSkateboardAchievement()
    {
        if (firstSkateboard && !firstSkateboardAchieved)
        {
            firstSkateboardAchieved = true;
            achievementEndUItext.GetComponent<Text>().text = "SOBRE RODES";
            achievementEndUIsubtext.GetComponent<Text>().text = "Ets la més cool!";
            Debug.Log("New Achievement: Sobre rodes!");
            achievementUI.SetActive(true);
            achievementShown = true;
            highSpeedSFX.Play();
            dimVolumes();
            lifeUp();
            StartCoroutine(hideachievement());
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
        coinCount = 0;
        coinCountDisplay.GetComponent<Text>().text = coinCount.ToString();
        lastAchievementText = "";
        highScoreText = "";

        treballadordelmes_coins_index = 0;
        seconds_to_elapse_index = 0;
        elapsedTime = 0f;
        achievementShown = false;
        runtimeHighScoreTriggered = false;
        firstAchievementMet = false;
        highScoreAchieved = false;
        firstForklift = false;
        firstForkliftAchieved = false;
        firstSkateboard = false;
        firstSkateboardAchieved = false;

        recentCoinTimes.Clear();

        achievementUI.SetActive(false);
        achievementEndUItext.GetComponent<Text>().text = "";
        achievementEndUIsubtext.GetComponent<Text>().text = "";

        SessionData.LoadHighScore(savingPlayerPrefences);
        highScore = SessionData.sessionHighScore;
    }
}