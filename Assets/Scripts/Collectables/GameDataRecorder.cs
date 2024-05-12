using UnityEngine;
using System;
using System.IO;

public class GameDataRecorder : MonoBehaviour
{
    public static GameDataRecorder instance;

    private string filePath;
    private StreamWriter writer;

    private int currentScore;
    private float gameDuration;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        filePath = Application.persistentDataPath + "/game_data.csv";

        // Create a new file or append to existing file
        writer = new StreamWriter(filePath, true);

        // Write header if the file is empty
        if (new FileInfo(filePath).Length == 0)
        {
            writer.WriteLine("Date,Score,Duration");
        }

        // Close the file to prevent locking
        writer.Close();

        // Subscribe to scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) // Change this index if Scene 0 has a different build index
        {
            // Save game data to CSV
            SaveGameData();
        }
    }

    public void RecordScore(int score)
    {
        currentScore = score;
    }

    public void RecordDuration(float duration)
    {
        gameDuration = duration;
    }

    private void SaveGameData()
    {
        // Open the file in append mode
        writer = new StreamWriter(filePath, true);

        // Create a new entry with current date, score, and duration
        string entry = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," + currentScore + "," + gameDuration;
        writer.WriteLine(entry);

        // Close the file to save changes
        writer.Close();
    }

    private void OnDestroy()
    {
        // Ensure the StreamWriter is closed when the object is destroyed
        if (writer != null)
        {
            writer.Close();
        }
    }
}
