using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrintCode : MonoBehaviour, IResettable
{
    public GameObject canvasText; // The parent object containing child Text objects
    public GameObject obstaclesDisplay;
    public Text obstaclesText;

    private string codePrompt = "";
    private string lastCodePrompt;
    private string obstaclesList = "";
    private bool isFadingOutObstacles = false;
    private float fadeDuration = 1.5f;
    private bool alreadyHitObstacle = false;
    private string lastExternalMessage = "";
    private Dictionary<string, string> printedCode = new Dictionary<string, string>
    {
        { "start",@"if (!startedRunning && Input.anyKey)
{
    BGM.Play();
    StartCoroutine(FadeMixerGroup.StartFade(audioMixer, volumeBGM, 3, 0.7f));
    StartCoroutine(PlayMainTheme());
    StartCoroutine(FadeMixerGroup.StartFade(audioMixer, volumeThemes, 1.5f, 1));
    StartCoroutine(FadeMixerGroup.StartFade(audioMixer, volumeSFX, 1.5f, 1));
    tutorial2d.transform.Find(touch-cards).gameObject.SetActive(false);
    startingText.SetActive(false);
}"
        },

            { "left", @"// moved left
if (!isFlying)
{
    if (pos == center)
        pos = left;

    else if (pos == right)
        pos = center;
}" },

        { "right", @"//moved right
if (!isFlying)
{
    if (pos == center)
        pos = right;

    else if (pos == left)
        pos = center;
}"
        },

        { "crouch", @"//crouch
if (!isRolling)
{
    SetCrouching(true);
    animator.SetBool(isrolling, true);
    StartCoroutine(RollSequence());
}"
        },

        { "hurt", "Player is hurt!"},

        {"fly", @"//Player is flying!!!

godmode = true;
flyFX.Play();
animator.SetBool(isflying, true);
isFlying = true;"
        },

        {"dead", "Player died." },

        { "jumpsequence", @"//jump
SetJumping(true);
jumpStarted = Time.time;
animator.SetBool(isjumping, true)"
        },

        { "dieonforklift", "Player was blown up."},

        {"longfall", "Player fell off the world."},

        { "jumpsequenceend", @"//jump end
float jumpDuration = 0.6f;
if (Time.time - jumpStarted >= jumpDuration)
{
    SetJumping(false);
    animator.SetBool(isjumping, false);
}"
        },

        {"nondi", "NDI source: " },

        {"panoptic", @"//entered panoptic
if (!alreadyCrossedPanoptic)
{
    StartCoroutine(ApplyGlissando());
    alreadyCrossedPanoptic = true;
}"
        }

    };

    private Text[] childTextComponents;
    private Dictionary<Text, float> textTimeouts = new Dictionary<Text, float>();
    private float displayDuration = 5.0f;

    void Start()
    {
        if (canvasText == null)
        {
            Debug.LogError("canvasText GameObject is not assigned.");
            return;
        }

        childTextComponents = canvasText.GetComponentsInChildren<Text>(true);
        if (childTextComponents.Length != 10)
            Debug.LogError("canvasText should have exactly 10 child objects with Text components.");

        foreach (Text text in childTextComponents)
        {
            text.gameObject.SetActive(false);
            textTimeouts[text] = 0f;
        }

        if (obstaclesText != null)
        {
            obstaclesText.text = "";
            SetTextAlpha(obstaclesText, 1f);
        }
    }

    void Update()
    {
        // Collect keys to reset after iteration
        List<Text> keysToReset = new List<Text>();

        foreach (var item in textTimeouts)
        {
            if (item.Value > 0f && Time.time > item.Value)
            {
                item.Key.gameObject.SetActive(false);
                keysToReset.Add(item.Key);
            }
        }

        // Modify the dictionary outside the loop
        foreach (Text key in keysToReset)
            textTimeouts[key] = 0f;

        if (!string.IsNullOrEmpty(codePrompt) && printedCode.TryGetValue(codePrompt, out string code))
        {
            DisplayRandomText(code);
            codePrompt = ""; // Reset codePrompt to avoid repeatedly setting text
        }

        if (!obstaclesDisplay.activeSelf && alreadyHitObstacle)
            obstaclesDisplay.SetActive(alreadyHitObstacle);

        if (isFadingOutObstacles && obstaclesText != null)
        {
            Color c = obstaclesText.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime / fadeDuration);
            obstaclesText.color = c;

            if (c.a <= 0.01f)
            {
                obstaclesText.text = "";
                isFadingOutObstacles = false;
                SetTextAlpha(obstaclesText, 1f);
            }
        }
    }

    public void SetCodePrompt(string newCodePrompt)
    {
        if (newCodePrompt != lastCodePrompt)
        {
            codePrompt = newCodePrompt;
            lastCodePrompt = newCodePrompt;
        }
    }

    public void UpdateObstacleList(string obstacles)
    {
        if (obstaclesText == null) return;
        if (!alreadyHitObstacle)
            alreadyHitObstacle = true;
        if (!string.IsNullOrEmpty(obstaclesList))
            obstaclesList += "\n" + obstacles;
        else
            obstaclesList = obstacles;
        obstaclesText.text = obstaclesList;
        SetTextAlpha(obstaclesText, 1f);
        isFadingOutObstacles = false;
    }

    public void DisplayExternalMessage(string message)
    {
        if (message == lastExternalMessage) return;
        lastExternalMessage = message;

        string targetText = "NDI source: " + message;
        Debug.Log(targetText);
        DisplayRandomText(targetText);
    }

    private void DisplayRandomText(string text)
    {
        // Find all currently inactive text components
        List<Text> inactiveTexts = new List<Text>();
        foreach (Text child in childTextComponents)
        {
            if (!child.gameObject.activeSelf)
                inactiveTexts.Add(child);
        }

        if (inactiveTexts.Count == 0)
        {
            Debug.LogWarning("No inactive text components available.");
            return;
        }

        // Randomly select one of the inactive children
        int randomIndex = Random.Range(0, inactiveTexts.Count);
        Text selectedChild = inactiveTexts[randomIndex];

        selectedChild.text = text;
        selectedChild.gameObject.SetActive(true);
        textTimeouts[selectedChild] = Time.time + displayDuration;
    }

    private void SetTextAlpha(Text t, float a)
    {
        Color c = t.color;
        c.a = a;
        t.color = c;
    }

    public void ResetState()
    {
        isFadingOutObstacles = true; // fade out
        alreadyHitObstacle = false;
        obstaclesList = "";
        obstaclesDisplay.SetActive(alreadyHitObstacle);
        obstaclesText.text = "";
        lastExternalMessage = "";
        SetTextAlpha(obstaclesText, 1f);
    }
}