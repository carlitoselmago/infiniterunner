using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


public class MineEntry : MonoBehaviour, IResettable
{
    public PlayerMove player;
    public bool isExit = false; // define entry or exit point
    public GameObject levelControl;
    public Volume mineVolume;
    private GenerateLevel generateLevel;
    private bool triggered = false;

    void Start()
    {
        if (levelControl != null)
            generateLevel = levelControl.GetComponent<GenerateLevel>();
        else
            Debug.LogError("MineEntry: levelControl not assigned!");
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            if (PlayerMove.onForklift) return;
            if (PlayerMove.onSkateboard)
                player.ClearSkateboard();
            triggered = true;

            if (!isExit)
            {
                // Entering the mine
                MineData.isInTheMine = true;
                MineData.endlessFallDisabled = true;
                //Debug.Log("Entering the mine");
                generateLevel.EnterMine(gameObject);
                PlayerMove.rayLength = 2f;
                //StartCoroutine(FadeVolume(0f, 1f, 3f));
            }
            else
            {
                // Exiting the mine
                MineData.isInTheMine = false;
                //Debug.Log("Exiting the mine");
                generateLevel.ExitMine();
                //StartCoroutine(FadeVolume(1f, 0f, 1.5f));
                StartCoroutine(ReenableEndlessFall());
            }
        }
    }

    IEnumerator ReenableEndlessFall()
    {
        yield return new WaitForSeconds(5);
        MineData.endlessFallDisabled = false;
    }

    public void ResetState()
    {
        triggered = false;
        //mineVolume.weight = 0f;
    }

}