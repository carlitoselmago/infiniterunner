using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public void Start()
    {
        Cursor.visible = false;
    }

    public void Restart()
    {
        // Find all active resettables and reset them
        foreach (var resettable in FindObjectsOfType<MonoBehaviour>().OfType<IResettable>())
            resettable.ResetState();
    }
}