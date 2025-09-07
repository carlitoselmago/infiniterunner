using System.Collections;
using UnityEngine;

public class MineEntry : MonoBehaviour
{
    public bool isExit = false; // define entry or exit point
    public GameObject levelControl;
    public GenerateSandstorm generateSandstorm;
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
            triggered = true;

            if (!isExit)
            {
                // Entering the mine
                MineData.isInTheMine = true;
                MineData.endlessFallDisabled = true;
                Debug.Log("Entering the mine");
                generateLevel.EnterMine();
                //generateSandstorm.StopTheSandstorm();
                //levelControl.GetComponent<GenerateSandstorm>().sandstormActive = false;
            }
            else
            {
                // Exiting the mine
                MineData.isInTheMine = false;
                Debug.Log("Exiting the mine");
                generateLevel.ExitMine();
                StartCoroutine(ReenableEndlessFall());
                //levelControl.GetComponent<GenerateSandstorm>().sandstormActive = true;
            }
        }
    }

    IEnumerator ReenableEndlessFall()
    {
        yield return new WaitForSeconds(5);
        MineData.endlessFallDisabled = false;
    }

}