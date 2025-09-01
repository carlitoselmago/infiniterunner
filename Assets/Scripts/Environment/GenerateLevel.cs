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

    public bool inMine = false;
    public int mineEntryIndex = 41;

    public GameObject MAP;
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
    {
        InstantiateInitialSection();
    }
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
        if (inMine) return;

        secNum = Random.Range(0, section.Length);

        // Skip mine entry prefab on surface
        while (secNum == mineEntryIndex)
        {
            secNum = Random.Range(0, section.Length);
        }

        GameObject newSection = Instantiate(section[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
        newSection.SetActive(true);
        newSection.transform.SetParent(MAP.transform, false);

        Chunk chunkData = newSection.GetComponent<Chunk>();
        if (chunkData != null)
        {
            // Update zPos using chunkLength
            //zPos += chunkData.chunkLength;
        }

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

        Chunk chunkData = newSection.GetComponent<Chunk>();
        if (chunkData == null)
        {
            Debug.LogError("Prefab missing Chunk component!");
            return;
        }

        // Update zPos using actual chunk length
        //zPos += chunkData.chunkLength;

        createdSections.Enqueue(newSection);
    }

    
    public void EnterMine()
    {
        inMine = true;
        Debug.Log("Generate Level entered mine");

        foreach (GameObject section in createdSections)
        {
            Chunk chunkData = section.GetComponent<Chunk>();
            if (chunkData == null) continue;

            // Disable surface chunks but skip mine entry
            if (section.transform.position.y == 0 && chunkData.chunkNum != mineEntryIndex)
            {
                section.SetActive(false);
            }
        }
    }


    public void ExitMine(Vector3 exitPosition)
    {
        //inMine = false;

        // Find the chunk length of the exit section
        // REPLACE
        Chunk exitChunk = GetComponent<Chunk>();
        int exitChunkLength = stepamount; // fallback

        if (exitChunk != null)
            exitChunkLength = exitChunk.chunkLength;

        // Reset zPos to continue correctly after the exit chunk
        zPos = Mathf.RoundToInt(exitPosition.z) + exitChunkLength;

        // Immediately generate a few chunks ahead
        for (int i = 0; i < 4; i++)
        {
            GenerateSection();
        }

        StartCoroutine(ReenableEndlessFall());
    }

    IEnumerator ReenableEndlessFall()
    {
        yield return new WaitForSeconds(3);
        inMine = false;
    }

}