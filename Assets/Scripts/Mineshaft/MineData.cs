using UnityEngine;

public class MineData : MonoBehaviour, IResettable
{
    public static bool isInTheMine = false;
    public static bool endlessFallDisabled = false;

    public void ResetState()

    {
        isInTheMine = false;
        endlessFallDisabled = false;
    }
}

