using UnityEngine;
using TMPro;
using System.IO;

public class LogPathUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField pathInput;
    public TMP_Text errorText;

    private bool panelOpen = false;

    void Start()
    {
        panel.SetActive(false);
        errorText.text = "";
    }

    void Update()
    {
        if (!panelOpen && Input.GetKeyDown(KeyCode.P))
        {
            TogglePanel();
            return;
        }

        if (!panelOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ValidateAndSave();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    private void TogglePanel()
    {
        if (!panelOpen) PlayerMove.paused = true;   // pause game when panel is open

        panelOpen = !panelOpen;
        panel.SetActive(panelOpen);

        if (panelOpen)
        {
            errorText.text = "";
            pathInput.text = PlayerPrefs.GetString(
                ThreadSafeFileLogger.PrefKey,
                ThreadSafeFileLogger.DefaultLogPath
            );

            pathInput.ActivateInputField();
            pathInput.Select();
        }
    }

    private void ClosePanel()
    {
        panelOpen = false;
        panel.SetActive(false);
        errorText.text = "";
        PlayerMove.paused = false;
    }

    private void ValidateAndSave()
    {
        string path = pathInput.text.Trim();

        if (!ValidatePath(path, out string error))
        {
            errorText.text = error;
            return;
        }

        PlayerPrefs.SetString(ThreadSafeFileLogger.PrefKey, path);
        PlayerPrefs.Save();

        ThreadSafeFileLogger.ReloadLogPath();
        ClosePanel();

        Debug.Log("Log path updated: " + path);
    }

    private bool ValidatePath(string path, out string error)
    {
        error = "";

        if (string.IsNullOrEmpty(path))
        {
            error = "Path cannot be empty.";
            return false;
        }

        try
        {
            string directory = Path.GetDirectoryName(path);
            string filename = Path.GetFileName(path);

            if (string.IsNullOrEmpty(filename))
            {
                error = "Invalid file name.";
                return false;
            }

            Directory.CreateDirectory(directory);

            using (FileStream fs = File.Open(path, FileMode.Append, FileAccess.Write))
            {
                fs.Close();
            }
        }
        catch (System.Exception e)
        {
            error = "Invalid or unwritable path:\n" + e.Message;
            return false;
        }

        return true;
    }
}