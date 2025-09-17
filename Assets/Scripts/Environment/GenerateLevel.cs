using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GenerateLevel : MonoBehaviour, IResettable
{
    public int stepamount = 100;
    public GameObject templatesparent;
    private GameObject[] sectionPrefabs;
    public AudioSource mainTheme;

    private int zPos;
    public int generatedSections = 0;
    public bool creatingSection = false;

    public static bool disableMinefall = false;
    public int mineEntryIndex = 40;
    private bool protectMineSection = false;
    private bool minePresent = false;

    public GameObject player;
    public GameObject MAP;
    private int resumeAhead = 450;
    public int secNum;

    // --- Active sections in play ---
    private Queue<GameObject> activeSections = new Queue<GameObject>();

    // --- Pool of reusable sections ---
    private Dictionary<int, Queue<GameObject>> sectionPools = new Dictionary<int, Queue<GameObject>>();

    void Awake()
    {
        Debug.Log("Started GenerateLevel");
        ResetZPos();
        CachePrefabs();
        for (int i = 0; i < 5; i++)
            InstantiateInitialSection();
    }

    void Update()
    {
        // Distance from MAP front (negative z) to the next section’s start
        int mapFront = -Mathf.RoundToInt(MAP.transform.position.z);

        if (mapFront + resumeAhead > zPos && !creatingSection) // adds spawn lookahead
        {
            creatingSection = true;
            GenerateSection();
            generatedSections++;
        }
    }

    public void UpdateZPos(int addedLength)
    {
        zPos += addedLength;
    }

    private void ResetZPos()
    {
        //zPos = 100; // start point
        zPos = 200; // start point
    }

    private void CachePrefabs()
    {
        sectionPrefabs = new GameObject[templatesparent.transform.childCount];
        for (int i = 0; i < templatesparent.transform.childCount; i++)
        {
            GameObject child = templatesparent.transform.GetChild(i).gameObject;
            child.transform.localPosition = Vector3.zero;
            child.SetActive(false);
            sectionPrefabs[i] = child;
            sectionPools[i] = new Queue<GameObject>();

            if (child.GetComponent<MineTemplateMarker>() != null)
            {
                mineEntryIndex = i;
                Debug.Log($"Detected mine template at index {i} (name '{child.name}')");
            }
        }
    }

    private void GenerateSection()
    {
        if (MineData.isInTheMine) { creatingSection = false; return; }

        secNum = Random.Range(0, sectionPrefabs.Length);

        // Only block mines if there are alternatives
        if (sectionPrefabs.Length > 1 && secNum == mineEntryIndex && minePresent)
        {
            // Force a non-mine prefab
            int tries = 0;
            while (secNum == mineEntryIndex && tries < 5) // max 5 tries to avoid infinite loop
            {
                secNum = Random.Range(0, sectionPrefabs.Length);
                tries++;
            }
        }

        GameObject newSection = GetFromPool(secNum);
        newSection.transform.position = new Vector3(0, 0, zPos);
        newSection.transform.SetParent(MAP.transform, false);
        newSection.SetActive(true);

        // Register section length after placement
        Chunk chunkData = newSection.GetComponent<Chunk>();
        if (chunkData != null)
            chunkData.RegisterLength(this);

            if (chunkData.chunkNum == mineEntryIndex)
            {
                minePresent = true;          // mine is now active
                protectMineSection = true;  // enable protection
            }

            activeSections.Enqueue(newSection);

        if (activeSections.Count > 8)
        {
            GameObject oldest = activeSections.Peek(); // look at first in queue
            Chunk oldestChunk = oldest.GetComponent<Chunk>();

            if (protectMineSection && oldestChunk != null && oldestChunk.chunkNum == mineEntryIndex)
            {
                // Keep the mine section until more are ahead
                Debug.Log("Keeping mine section in queue for now.");
            }
            else
            {
                oldest = activeSections.Dequeue();
                ReturnToPool(oldest);
                protectMineSection = false; // after this, allow normal cleanup
                minePresent = false;
            }
        }

        creatingSection = false;
    }

    private void InstantiateInitialSection()
    {
        secNum = Random.Range(0, sectionPrefabs.Length);
        GameObject newSection = GetFromPool(secNum);
        newSection.transform.position = new Vector3(0, 0, zPos);
        newSection.transform.SetParent(MAP.transform, false);
        newSection.SetActive(true);

        // Register section length after placement
        Chunk chunkData = newSection.GetComponent<Chunk>();
        if (chunkData != null)
            chunkData.RegisterLength(this);

        activeSections.Enqueue(newSection);
    }

    public void EnterMine()
    {
        //Debug.Log("Generate Level entered mine");
        minePresent = false;
        //Debug.Log($"EnterMine() — activeSections count before: {activeSections.Count}");
        Queue<GameObject> newActive = new Queue<GameObject>();
        GameObject mineEntry = null;

        if (mineEntry != null)
        {
            newActive.Enqueue(mineEntry);
            protectMineSection = true; // start protection
        }

        while (activeSections.Count > 0)
        {
            GameObject section = activeSections.Dequeue();
            Chunk chunkData = section.GetComponent<Chunk>();
            int num = (chunkData != null) ? chunkData.chunkNum : -999;
            //Debug.Log($" Checking section '{section.name}' chunkNum={num}");


            if (chunkData == null)
            {
                Debug.LogWarning($"  Section {section.name} has no Chunk component — returning to pool.");
                ReturnToPool(section);
                continue;
            }

            if (chunkData.chunkNum != mineEntryIndex)
                ReturnToPool(section);
            else
            {
                mineEntry = section;
                //Debug.Log(" --> Found mine entry here!");
            }
        }

        if (mineEntry != null)
            newActive.Enqueue(mineEntry);

        //Debug.Log($"EnterMine() — newActive count after: {newActive.Count}");
        activeSections = newActive;
    }

    public void ExitMine()
    {
        int mapFront = -Mathf.RoundToInt(MAP.transform.position.z);
        zPos = mapFront + 100; // just a safety buffer

        creatingSection = false;

        // Start staged resume
        StartCoroutine(ResumeGenerationStaggered(4, 0.2f));
    }

    //experimental: this is the olf version
    /*
    public void ExitMine()
    {
        int mapFront = -Mathf.RoundToInt(MAP.transform.position.z);
        zPos = mapFront + 100; // just a safety buffer (resume ahead)

        //Debug.Log($"ExitMine: MAP.z = {MAP.transform.position.z}, mapFront = {mapFront}, zPos set to {zPos}");

        creatingSection = false;
    }*/

    /// <summary>
    /// Staggers the generation of initial sections after exiting the mine.
    /// </summary>
    /// <param name="count">How many sections to spawn</param>
    /// <param name="delay">Delay between each spawn (seconds)</param>

    IEnumerator ResumeGenerationStaggered(int count, float delay)
    {
        for (int i = 0; i < count; i++)
        {
            GenerateSection();
            generatedSections++;
            yield return new WaitForSeconds(delay);
        }

        // After stagged resume, normal Update() spawning will take over
        creatingSection = false;
    }



    // --- Pool management ---
    private GameObject GetFromPool(int prefabIndex)
    {
        GameObject obj;
        if (sectionPools[prefabIndex].Count > 0)
            obj = sectionPools[prefabIndex].Dequeue();
        else
        {
            obj = Instantiate(sectionPrefabs[prefabIndex]);
            // Make sure the prefab knows its index
            obj.GetComponent<Chunk>().chunkNum = prefabIndex;
        }

        // Reset everything that supports IResettable
        foreach (var reset in obj.GetComponentsInChildren<IResettable>(true))
            reset.ResetState();

        obj.SetActive(true); // ensures OnEnable runs
        return obj;
    }

    private void ReturnToPool(GameObject section)
    {
        section.SetActive(false);
        section.transform.SetParent(null);

        Chunk chunkData = section.GetComponent<Chunk>();
        if (chunkData != null)
            sectionPools[chunkData.chunkNum].Enqueue(section);
        else
            Destroy(section);
    }

    public void ResetState()
    {
        Debug.Log("GenerateLevel Reset");

        while (activeSections.Count > 0)
        {
            GameObject section = activeSections.Dequeue();
            ReturnToPool(section);
        }

        ResetZPos();
        generatedSections = 0;
        creatingSection = false;
        disableMinefall = false;

        MAP.transform.position = Vector3.zero;

        for (int i = 0; i < 5; i++)
            InstantiateInitialSection();
    }
}


/*
 * 
 * 
 * 
 * // OLD SCRIPT
 * 
 * 
 * 
 * 
 * using System.Collections.Generic;
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

}*/