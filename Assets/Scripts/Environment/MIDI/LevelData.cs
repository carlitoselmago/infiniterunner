using System;

[Serializable]
public class Note
{
    public float time;      // seconds from song start
    public int lane;        // 1 = low, 2 = mid, 3 = high
    public float duration;  // hold length in seconds
}

[Serializable]
public class LevelData
{
    public float bpm;
    public Note[] notes;
}
