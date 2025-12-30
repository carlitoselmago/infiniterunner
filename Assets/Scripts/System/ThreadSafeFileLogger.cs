using UnityEngine;
using System.IO;
using System.Text;

public class ThreadSafeFileLogger : MonoBehaviour
{
    private static readonly object fileLock = new object();
    private static StreamWriter writer;
    private static bool initialized;
    public static bool logging = false; // logging enabled

    public const string PrefKey = "CustomLogPath";

    public static string DefaultLogPath
    {
        get
        {
#if UNITY_STANDALONE_WIN
            return @"C:\Users\laboratori\Desktop\Logs\debug_log.txt";
#else
            return Path.Combine(Application.persistentDataPath, "debug_log.txt");
#endif
        }
    }

    void Awake()
    {
        if (initialized) return;
        initialized = true;

        DontDestroyOnLoad(gameObject);

        if (!logging) return;
        OpenLogFile();
        Application.logMessageReceivedThreaded += HandleLog;
    }

    void OnApplicationQuit()
    {
        if (!logging) return;

        Application.logMessageReceivedThreaded -= HandleLog;
        CloseLogFile();
    }

    public static void ReloadLogPath()
    {
        if (!logging) return;

        CloseLogFile();
        OpenLogFile();
    }

    private static void OpenLogFile()
    {
        lock (fileLock)
        {
            string path = PlayerPrefs.GetString(PrefKey, DefaultLogPath);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                writer = new StreamWriter(path, true, Encoding.UTF8);
                writer.AutoFlush = true;

                writer.WriteLine("========================================");
                writer.WriteLine($"LOG STARTED: {System.DateTime.Now}");
                writer.WriteLine($"Unity {Application.unityVersion}");
                writer.WriteLine($"Platform: {Application.platform}");
                writer.WriteLine("========================================");
            }
            catch (System.Exception e)
            {
                writer = null;
                Debug.LogError("Failed to open log file: " + e.Message);
            }
        }
    }

    private static void CloseLogFile()
    {
        lock (fileLock)
        {
            writer?.Flush();
            writer?.Close();
            writer = null;
        }
    }

    private static void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (!logging) return;

        lock (fileLock)
        {
            if (writer == null) return;

            writer.WriteLine(
                $"[{System.DateTime.Now:HH:mm:ss.fff}] [{type}] {logString}"
            );

            if (type == LogType.Exception || type == LogType.Error)
                writer.WriteLine(stackTrace);
        }
    }
}
