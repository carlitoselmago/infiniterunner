using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public int stepamount = 100;
    public GameObject templatesparent;
    private GameObject[] section;
    public AudioSource mainTheme;
    
    private int zPos;
    public int generatedSections = 0;
    public bool creatingSection = false;

    public static bool disableMinefall = false;
    public int mineEntryIndex = 41;

    public GameObject player;
    public GameObject MAP;
    private int resumeAhead = 530;
    public int secNum;

    // Queue to store references to the instantiated sections
    private Queue<GameObject> createdSections = new Queue<GameObject>();

   void Start()
    {
    zPos =200;// stepamount*2;

    // Initialize the section array with the number of children in templatesparent
    section = new GameObject[templatesparent.transform.childCount];

    // Loop through each child, set its position to 0,0,0, and add it to the section array
    for (int i = 0; i < templatesparent.transform.childCount; i++)
        {
        GameObject child = templatesparent.transform.GetChild(i).gameObject;
        child.transform.localPosition = Vector3.zero;  // Reset the position of each child
        child.SetActive(false); // Set to inactive until it is instantiated
        section[i] = child;
        }

    // Preload 4 sections at the start of the game
    for (int i = 0; i < 5; i++)
        InstantiateInitialSection();
    }

  void Update()
    {
        // Check if the map has moved enough to require a new section
        if (MAP.transform.position.z < -zPos + (stepamount*4) && !creatingSection)
        {
            creatingSection = true;
            GenerateSection();
            generatedSections ++;
        }
    }

    public void UpdateZPos(int addedLength)
    {
        zPos += addedLength;
    }

    void GenerateSection()
    {
        if (MineData.isInTheMine) return;

        secNum = Random.Range(0, section.Length);
        GameObject newSection = Instantiate(section[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
        newSection.SetActive(true);
        newSection.transform.SetParent(MAP.transform, false);
        createdSections.Enqueue(newSection);

        if (createdSections.Count > 8)
        {
            GameObject oldSection = createdSections.Dequeue();
            oldSection.SetActive(false);
        }

        creatingSection = false;
    }


    void InstantiateInitialSection()
    {
        secNum = Random.Range(0, section.Length);
        GameObject newSection = Instantiate(section[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
        newSection.SetActive(true);

        // Set the parent of the instantiated child to MAP
        newSection.transform.SetParent(MAP.transform, false);
        createdSections.Enqueue(newSection);
    }
    
    public void EnterMine()
    {
        Debug.Log("Generate Level entered mine");
        GameObject mineEntry = null;

        foreach (GameObject section in createdSections)
        {
            Chunk chunkData = section.GetComponent<Chunk>();
            if (chunkData == null) continue;

            if (chunkData.chunkNum != mineEntryIndex)
            {
                section.SetActive(false);
            } else {
                mineEntry = section;
            }
        }
        createdSections.Clear();
        if (mineEntry != null)
            createdSections.Enqueue(mineEntry);
    }

    public void ExitMine()
    {
        zPos = Mathf.RoundToInt(player.transform.position.z) + resumeAhead;
        Debug.Log($"ExitMine: player.z = {player.transform.position.z}, zPos set to {zPos}");
        creatingSection = true;
        Debug.Log($"Exited mine. Resuming generation at z = {zPos}");
        StartCoroutine(ResumeGenerationNextFrame());
    }

    IEnumerator ResumeGenerationNextFrame()
    {
        yield return null; // wait 1 frame
        creatingSection = false;
    }

}