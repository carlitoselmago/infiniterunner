using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrintCode : MonoBehaviour
{
    public GameObject canvasText; // The parent object containing child Text objects

    private string codePrompt = "";
    private string lastCodePrompt;
    private Dictionary<string, string> printedCode = new Dictionary<string, string>
    {
        { "start","if (startedrunning == false && Input.anyKey == true)\n" +
            "{\n" +
            "\t\tBGM.Play();\n" +
            "\t\tStartCoroutine(FadeMixerGroup.StartFade(audioMixer, volumeBGM, duration = 3, targetVolume = 0.7f)); \n" +
            "\t\tStartCoroutine(PlayMainTheme());\n" +
            "\t\tStartCoroutine(FadeMixerGroup.StartFade(audioMixer, volumeThemes, duration = 1.5f, targetVolume = 1));\n" +
            "\t\tStartCoroutine(FadeMixerGroup.StartFade(audioMixer, volumeSFX, duration = 1.5f, targetVolume = 1));\n" +
            "\t\ttutorial2d.transform.Find(touch-cards).gameObject.SetActive(false);\n" +
            "\t\tstartingText.SetActive(false);" +
            "}"
        },

        { "left", "//moved left\n" +
            "if (!isFlying)\n" +
            "{\n" +
            "\tif (pos == center && transform.position.x == 0f)\n" +
            "\t{\n" +
            "\t\tpos = left;\n" +
            "\t}\n" +
            "\telse if (pos == right)\n" +
            "\t{\n" +
            "\t\tpos = center;\n" +
            "\t}"
        },

        { "right", "//moved right\n" +
            "if (!isFlying)\n" +
            "{\n" +
            "\tif (pos == center && transform.position.x == 0f)\n" +
            "\t{\n" +
            "\t\tpos = right;\n" +
            "\t}\n" +
            "\telse if (pos == left)\n" +
            "\t{\n" +
            "\t\tpos = center;\n" +
            "\t}"
        },

        { "crouch", "//crouch\n"+
            "if (isRolling == false)\n" +
            "{\n" +
            "\t\tcrouchhitbox();\n" +
            "\t\tanimator.SetBool(isrolling, true);\n" +
            "\t\tStartCoroutine(RollSequence());"
        },

        { "rollsequence", "IEnumerator RollSequence()\n" +
            "{\n" +
            "\tyield return new WaitForSeconds(0.45f);\n" +
            "\tstandingUp = true;\n" +
            "\tyield return new WaitForSeconds(0.45f);\n" +
            "\tisRolling = false;\n" +
            "\tanimator.SetBool(isrolling, false);\n" +
            "\tnormalhitbox();\n" +
            "}"
        },

        { "hurt", "Player is hurt!"},

        {"fly", "//Player is flying!!!\n" +
            "\n" +
            "godmode = true;\n" +
            "flyFX.Play();\n" +
            "animator.SetBool(isflying, true);\n" +
            "isFlying = true;"
            },

        {"dead", "Player died." },

        { "jumpsequence", "//jump\n" +
            "IEnumerator JumpSequence()\n" +
            "{\n" +
            "\tjumphitbox();\n" +
            "\tyield return new WaitForSeconds(0.30f);\n" +
            "\tcomingDown = true;\n" +
            "\tyield return new WaitForSeconds(0.30f);\n" +
            "\tisJumping = false;\n" +
            "\tcomingDown = false;\n" +
            "\tanimator.SetBool(isjumping, false);\n" +
            "\tnormalhitbox();\n" +
            "}"
        },

        {"panoptic", "//entered panoptic\n" +
            "if (alreadyCrossedPanoptic == false)\n" +
            "{\n" +
            "\tStartCoroutine(ApplyGlissando());\n" +
            "\talreadyCrossedPanoptic = true;\n" +
            "}"
        }

    };

    private Text[] childTextComponents;
    private Dictionary<Text, float> textTimeouts = new Dictionary<Text, float>();
    private float displayDuration = 5.0f; // Duration for which the text will be displayed

    void Start()
    {
        if (canvasText == null)
        {
            Debug.LogError("canvasText GameObject is not assigned.");
            return;
        }

        // Get all Text components from child objects
        childTextComponents = canvasText.GetComponentsInChildren<Text>(true);

        if (childTextComponents.Length != 10)
        {
            Debug.LogError("canvasText should have exactly 10 child objects with Text components.");
        }

        // Ensure all children are initially inactive and setup their timeout
        foreach (Text text in childTextComponents)
        {
            text.gameObject.SetActive(false);
            textTimeouts[text] = 0f;
        }
    }

    //CHATGPT FIX
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
        {
            textTimeouts[key] = 0f;
        }

        if (!string.IsNullOrEmpty(codePrompt) && printedCode.TryGetValue(codePrompt, out string code))
        {
            DisplayRandomText(code);
            codePrompt = ""; // Reset codePrompt to avoid repeatedly setting text
        }
    }

    /*
     * PREVIOUS VERSION (MAYBE CAUSED ERROR
    void Update()
    {
        // Update timeouts and deactivate texts that have timed out
        foreach (var item in textTimeouts)
        {
            if (item.Value > 0f && Time.time > item.Value)
            {
                item.Key.gameObject.SetActive(false);
                textTimeouts[item.Key] = 0f; // Reset the timeout
            }
        }

        if (!string.IsNullOrEmpty(codePrompt) && printedCode.TryGetValue(codePrompt, out string code))
        {
            DisplayRandomText(code);
            codePrompt = ""; // Reset codePrompt to avoid repeatedly setting text
        }
    }*/

    public void SetCodePrompt(string newCodePrompt)
    {
        if (newCodePrompt != lastCodePrompt)
        {
            codePrompt = newCodePrompt;
            lastCodePrompt = newCodePrompt;
        }
    }

    private void DisplayRandomText(string text)
    {
        // Find all currently inactive text components
        List<Text> inactiveTexts = new List<Text>();
        foreach (Text child in childTextComponents)
        {
            if (!child.gameObject.activeSelf)
            {
                inactiveTexts.Add(child);
            }
        }

        if (inactiveTexts.Count == 0)
        {
            Debug.LogWarning("No inactive text components available.");
            return;
        }

        // Randomly select one of the inactive children
        int randomIndex = Random.Range(0, inactiveTexts.Count);
        Text selectedChild = inactiveTexts[randomIndex];

        // Set the text and activate the selected child
        selectedChild.text = text;
        selectedChild.gameObject.SetActive(true);

        // Set the timeout for this text
        textTimeouts[selectedChild] = Time.time + displayDuration;
    }
}
