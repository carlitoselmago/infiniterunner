using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrintCode : MonoBehaviour, IResettable
{
    public GameObject canvasText; // The parent object containing child Text objects
    public GameObject obstaclesDisplay;
    public Text obstaclesText;

    //private string codePrompt = "";
    //private string lastCodePrompt;
    private bool isFadingOutObstacles = false;
    private float fadeDuration = 1.5f;
    private bool alreadyHitObstacle = false;
    private string lastExternalMessage = "";

    private readonly List<Text> keysToReset = new List<Text>(10);
    private readonly List<Text> inactiveTexts = new List<Text>(10);
    private readonly System.Text.StringBuilder obstaclesBuilder =
        new System.Text.StringBuilder(256);
    private readonly Queue<string> codePromptQueue = new Queue<string>(16);
    private string lastPrintedCode = null;
    private string lastEnqueuedCode = null;

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
            //Debug.LogError("canvasText GameObject is not assigned.");
            return;
        }

        childTextComponents = canvasText.GetComponentsInChildren<Text>(true);
        if (childTextComponents.Length != 10)
            //Debug.LogError("canvasText should have exactly 10 child objects with Text components.");

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

    /*void Update()
    {
        keysToReset.Clear();

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
    }*/
    void Update()
    {
        keysToReset.Clear();

        foreach (var t in childTextComponents)
        {
            if (t == null) continue;

            if (!textTimeouts.ContainsKey(t))
                textTimeouts[t] = 0f;
        }

        foreach (var kvp in textTimeouts)
        {
            if (kvp.Value > 0f && Time.time > kvp.Value)
            {
                kvp.Key.gameObject.SetActive(false);
                keysToReset.Add(kvp.Key);
            }
        }

        for (int i = 0; i < keysToReset.Count; i++)
            textTimeouts[keysToReset[i]] = 0f;
        /*
        if (!string.IsNullOrEmpty(codePrompt) &&
            printedCode.TryGetValue(codePrompt, out string code))
        {
            DisplayRandomText(code);
            codePrompt = "";
        }*/
        if (codePromptQueue.Count > 0)
        {
            string prompt = codePromptQueue.Dequeue();

            if (prompt == lastPrintedCode)
                return;

            if (printedCode.TryGetValue(prompt, out string code))
            {
                DisplayRandomText(code);
                lastPrintedCode = prompt;
            }
        }

        if (!obstaclesDisplay.activeSelf && alreadyHitObstacle)
            obstaclesDisplay.SetActive(true);

        UpdateObstacleFade();
    }

    private void UpdateObstacleFade()
    {
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
        if (string.IsNullOrEmpty(newCodePrompt))
            return;

        //if (newCodePrompt == lastReceivedCode)
        //    return;

        if (newCodePrompt == lastEnqueuedCode)
            return;
        lastEnqueuedCode = newCodePrompt;
        codePromptQueue.Enqueue(newCodePrompt);
        //lastReceivedCode = newCodePrompt;
        //codePrompt = newCodePrompt;
    }

    public void UpdateObstacleList(string obstacles)
    {
        if (obstaclesText == null) return;
        alreadyHitObstacle = true;
        if (obstaclesBuilder.Length>0)
            obstaclesBuilder.Append('\n');
        obstaclesBuilder.Append(obstacles);
        obstaclesText.text = obstaclesBuilder.ToString();
        SetTextAlpha(obstaclesText, 1f);
        isFadingOutObstacles = false;
    }

    public void DisplayExternalMessage(string message)
    {
        if (message == lastExternalMessage) return;
        lastExternalMessage = message;

        string targetText = "NDI source: " + message;
        //Debug.Log(targetText);
        DisplayRandomText(targetText);
    }

    private void DisplayRandomText(string text)
    {
        inactiveTexts.Clear();

        for (int i = 0; i < childTextComponents.Length; i++)
        {
            if (!childTextComponents[i].gameObject.activeSelf)
                inactiveTexts.Add(childTextComponents[i]);
        }

        if (inactiveTexts.Count == 0)
        {
            Text fallback = null;
            float oldestTime = float.MaxValue;

            foreach (var t in childTextComponents)
            {
                if (t == null) continue;

                float timeout = textTimeouts.TryGetValue(t, out float v) ? v : 0f;

                if (timeout < oldestTime)
                {
                    oldestTime = timeout;
                    fallback = t;
                }
            }


            if (fallback == null)
            {
                Debug.LogError("No valid fallback Text found");
                return;
            }

            fallback.text = text;
            fallback.gameObject.SetActive(true);
            textTimeouts[fallback] = Time.time + displayDuration;
            return;
        }


        int randomIndex = Random.Range(0, inactiveTexts.Count);
        Text selected = inactiveTexts[randomIndex];

        selected.text = text;
        selected.gameObject.SetActive(true);
        textTimeouts[selected] = Time.time + displayDuration;
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
        codePromptQueue.Clear();
        lastEnqueuedCode = null;
        lastPrintedCode = null;
        obstaclesBuilder.Clear();
        obstaclesDisplay.SetActive(alreadyHitObstacle);
        obstaclesText.text = "";
        lastExternalMessage = "";
        SetTextAlpha(obstaclesText, 1f);
    }
}