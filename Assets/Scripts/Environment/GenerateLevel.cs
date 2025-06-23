using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public int stepamount=100;
    public GameObject templatesparent;
    private GameObject[] section;
    public AudioSource mainTheme;
    
    private int zPos;
    public int generatedSections = 0;
    public bool creatingSection = false;

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

    // Preload 3 sections at the start of the game
    for (int i = 0; i < 4; i++)
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

    void GenerateSection()
    {
        secNum = Random.Range(0, section.Length);

        if (secNum == 28 || secNum == 33)   // secNum 28 and 29 correspond to template29 and 30 (double length section)
        {
            stepamount = 200;
            Debug.Log($"Double section created: {secNum}");
            Debug.Log(secNum);
        }
        else if (secNum == 29)
        {
            stepamount = 215;
            Debug.Log("Train section created");
        }
        else
        {
            stepamount = 100;
        }

        GameObject newSection = Instantiate(section[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
        newSection.SetActive(true);

        newSection.transform.SetParent(MAP.transform, false);
        zPos += stepamount;
        createdSections.Enqueue(newSection);  // Add the new section to the queue

        if (createdSections.Count > 6)
        {
            GameObject oldSection = createdSections.Dequeue();  // Remove the oldest section from the queue
            Destroy(oldSection);  // Destroy the oldest section object
        }
        creatingSection = false;
    }

      void InstantiateInitialSection()
    {
        secNum = Random.Range(0, section.Length);

        if (secNum == 28 || secNum == 33)   // secNum 28 and 29 correspond to template29 and 30 (double length section)
        {
            stepamount = 200;
            Debug.Log("Double section created");
            Debug.Log(secNum);
        } else if (secNum == 29)
        {
            stepamount = 215;
            Debug.Log("Train section created");
        }
        else
        {
            stepamount = 100;
        }

        GameObject newSection = Instantiate(section[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
        newSection.SetActive(true);

        // Set the parent of the instantiated child to MAP
        newSection.transform.SetParent(MAP.transform, false);
        zPos += stepamount;
        createdSections.Enqueue(newSection);
    }
}
/*
using System.Collections.Generic;
using UnityEngine;


public class GenerateLevel : MonoBehaviour
{
    public int stepamount=100;
    public GameObject templatesparent;
    private GameObject[] section;
    public AudioSource mainTheme;

    //private int zPos;
    private Vector3 currentPos;
    private Vector3 currentDir = Vector3.forward;
    public int generatedSections = 0;
    public bool creatingSection = false;
    private float lastGeneratedZ = 0f;  // add at the top


    public GameObject MAP;  // MAP tracks the map movement
    public GameObject BLOCKS;   // BLOCKS is the parent for the template clones
    public int secNum;  // Number of template (in the array)

    // Queue to store references to the instantiated sections
    private Queue<GameObject> createdSections = new Queue<GameObject>();

   void Start()
{
        //zPos =200;// stepamount*2;
        currentPos = new Vector3(0, 0, 200);

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

    // Preload 3 sections at the start of the game
    for (int i = 0; i < 4; i++)
    {
        InstantiateInitialSection();
    }
}


    void Update()
    {
        float mapZ = MAP.transform.position.z;

        // Generate section only if MAP has moved forward significantly past the last generation point
        if (lastGeneratedZ - mapZ > stepamount && !creatingSection)
        {
            creatingSection = true;
            GenerateSection();
            lastGeneratedZ = mapZ;
        }
    }


    void GenerateSection()
    {
        secNum = Random.Range(0, section.Length);

        GameObject prefab = section[secNum];

        GameObject newChunk = Instantiate(prefab, currentPos, Quaternion.LookRotation(currentDir));
        newChunk.SetActive(true);
        newChunk.transform.SetParent(BLOCKS.transform, false);

        Chunk chunkData = newChunk.GetComponent<Chunk>();

        if (chunkData != null && chunkData.endPoint != null)
        {
            currentPos = chunkData.endPoint.position;
            currentDir = chunkData.endPoint.forward;
        } else
        {
            currentPos += currentDir * stepamount;
        }

        createdSections.Enqueue(newChunk);  // Add the new section to the queue

        if (createdSections.Count > 6)
        {
            GameObject oldSection = createdSections.Dequeue();  // Remove the oldest section from the queue
            Destroy(oldSection);  // Destroy the oldest section object
        }
        creatingSection = false;
    }

      void InstantiateInitialSection()
    {
        secNum = Random.Range(0, section.Length);

        GameObject prefab = section[secNum];

        GameObject newChunk = Instantiate(prefab, currentPos, Quaternion.LookRotation(currentDir));
        newChunk.SetActive(true);
        newChunk.transform.SetParent(BLOCKS.transform, false);

        Chunk chunkData = newChunk.GetComponent<Chunk>();

        if (chunkData != null && chunkData.endPoint != null)
        {
            currentPos = chunkData.endPoint.position;
            currentDir = chunkData.endPoint.forward;
        }
        else
        {
            currentPos += currentDir * stepamount;
        }

        createdSections.Enqueue(newChunk);  // Add the new section to the queue
    }
}*/