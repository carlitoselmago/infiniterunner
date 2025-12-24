using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public void Start()
    {
        ApplyGameStateCursor();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
            ApplyGameStateCursor();
    }

    private void ApplyGameStateCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Restart()
    {
        // Find all active resettables and reset them
        foreach (var resettable in FindObjectsOfType<MonoBehaviour>(true).OfType<IResettable>())
            resettable.ResetState();
    }
}