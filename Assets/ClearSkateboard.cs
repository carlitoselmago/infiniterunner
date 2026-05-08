using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


public class ClearSkateboard : MonoBehaviour, IResettable
{
    public PlayerMove player;

    private bool triggered = false;


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            if (PlayerMove.onForklift) return;
            if (PlayerMove.onSkateboard)
                player.ClearSkateboard();
            triggered = true;
        }
    }

    public void ResetState()
    {
        triggered = false;
    }

}