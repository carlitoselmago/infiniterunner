using UnityEngine;

public static class SessionData
{
    private const string HighScoreKey = "HighScore";
    public static int sessionHighScore = 0;
    public static bool minePresent = false;

    public static void LoadHighScore(bool usePrefs)
    {
        sessionHighScore = usePrefs ? PlayerPrefs.GetInt(HighScoreKey, 0) : 0;
    }

    public static void UpdateHighScore(int score, bool usePrefs)
    {
        sessionHighScore = score;
        Debug.Log("New High Score! " + score);
        if (usePrefs)
        {
            PlayerPrefs.SetInt(HighScoreKey, sessionHighScore);
            PlayerPrefs.Save();
        }
    }

    public static void ClearHighScore()
    {
        sessionHighScore = 0;
        PlayerPrefs.DeleteKey(HighScoreKey);
    }
}