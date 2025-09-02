using System.Collections;
using UnityEngine;

public class MineEntry : MonoBehaviour
{
    public bool isExit = false; // define entry or exit point
    public GameObject levelControl;
    private GenerateLevel generateLevel; // centralized script for inMine bool

    void Start()
    {
        if (levelControl != null)
        {
            generateLevel = levelControl.GetComponent<GenerateLevel>();
        }
        else
        {
            Debug.LogError("MineEntry: levelControl not assigned!");
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!isExit)
            {
                // Entering the mine
                Debug.Log("Entering the mine");
                generateLevel.EnterMine();
                levelControl.GetComponent<GenerateSandstorm>().sandstormActive = false;
            }
            else
            {
                // Exiting the mine
                Debug.Log("Exiting the mine");
                generateLevel.ExitMine(transform.position);
                //experimental
                //Chunk exitChunk = GetComponent<Chunk>();
                //generateLevel.ExitMine(exitChunk);
                levelControl.GetComponent<GenerateSandstorm>().sandstormActive = true;
            }
        }
    }

}