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
    public int mineEntryIndex = 41;

    public GameObject player;
    public GameObject MAP;
    private int resumeAhead = 530;
    public int secNum;

    // --- Active sections in play ---
    private Queue<GameObject> activeSections = new Queue<GameObject>();

    // --- Pool of reusable sections ---
    private Dictionary<int, Queue<GameObject>> sectionPools = new Dictionary<int, Queue<GameObject>>();

    void Start()
    {
        Debug.Log("Started GenerateLevel");
        ResetZPos();
        CachePrefabs();
        for (int i = 0; i < 5; i++)
            InstantiateInitialSection();
    }

    void Update()
    {
        if (MAP.transform.position.z < -zPos + (stepamount * 4) && !creatingSection)
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
        zPos = 100; // start point
        //zPos = 200; // start point
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
        }
    }

    private void GenerateSection()
    {
        if (MineData.isInTheMine) { creatingSection = false; return; }

        secNum = Random.Range(0, sectionPrefabs.Length);
        GameObject newSection = GetFromPool(secNum);
        newSection.transform.position = new Vector3(0, 0, zPos);
        newSection.transform.SetParent(MAP.transform, false);
        newSection.SetActive(true);

        activeSections.Enqueue(newSection);

        if (activeSections.Count > 8)
        {
            GameObject oldSection = activeSections.Dequeue();
            ReturnToPool(oldSection);
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

        activeSections.Enqueue(newSection);
    }

    public void EnterMine()
    {
        Debug.Log("Generate Level entered mine");
        Queue<GameObject> newActive = new Queue<GameObject>();
        GameObject mineEntry = null;

        while (activeSections.Count > 0)
        {
            GameObject section = activeSections.Dequeue();
            Chunk chunkData = section.GetComponent<Chunk>();
            if (chunkData == null) continue;

            if (chunkData.chunkNum != mineEntryIndex)
                ReturnToPool(section);
            else
                mineEntry = section;
        }

        if (mineEntry != null)
            newActive.Enqueue(mineEntry);

        activeSections = newActive;
    }

    public void ExitMine()
    {
        zPos = Mathf.RoundToInt(player.transform.position.z) + resumeAhead;
        Debug.Log($"ExitMine: player.z = {player.transform.position.z}, zPos set to {zPos}");
        creatingSection = true;
        StartCoroutine(ResumeGenerationNextFrame());
    }

    IEnumerator ResumeGenerationNextFrame()
    {
        yield return null;
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
            obj.GetComponent<Chunk>().chunkNum = prefabIndex + 1; // added 1 to compensate array's index 0
        }

        // probably remove?
        /*var reset = obj.GetComponent<ResettableSection>();
        if (reset != null)
            reset.ResetSection();*/

        // Reset everything that supports IResettable
        foreach (var reset in obj.GetComponentsInChildren<IResettableChild>(true))
        {
            reset.ResetState();
        }

        obj.SetActive(true); // ensures OnEnable runs
        return obj;

        /*
        if (sectionPools[prefabIndex].Count > 0)
            return sectionPools[prefabIndex].Dequeue();
        else
        {
            GameObject obj = Instantiate(sectionPrefabs[prefabIndex]);
            obj.SetActive(false);
            obj.GetComponent<Chunk>().chunkNum = prefabIndex;
            return obj;
        }*/
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




/*using System.Collections.Generic;
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
    public int mineEntryIndex = 41;

    public GameObject player;
    public GameObject MAP;
    private int resumeAhead = 530;
    public int secNum;

    // --- Active sections in play ---
    private Queue<GameObject> activeSections = new Queue<GameObject>();

    // --- Pool of reusable sections ---
    private Dictionary<int, Queue<GameObject>> sectionPools = new Dictionary<int, Queue<GameObject>>();

    void Start()
    {
    zPos =200;// stepamount*2;

    // Cache section prefabs
    sectionPrefabs = new GameObject[templatesparent.transform.childCount];

    for (int i = 0; i < templatesparent.transform.childCount; i++)
        {
        GameObject child = templatesparent.transform.GetChild(i).gameObject;
        child.transform.localPosition = Vector3.zero;  // Reset the position of each child
        child.SetActive(false); // Set to inactive until it is instantiated
        sectionPrefabs[i] = child;
            sectionPools[i] = new Queue<GameObject>();//one pool per prefab
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

        secNum = Random.Range(0, sectionPrefabs.Length);

        GameObject newSection = GetFromPool(secNum);
        newSection.transform.position = new Vector3(0, 0, zPos);
        newSection.transform.SetParent(MAP.transform, false);
        newSection.SetActive(true);

        activeSections.Enqueue(newSection);

        if (activeSections.Count > 8)
        {
            GameObject oldSection = activeSections.Dequeue();
            ReturnToPool(oldSection);
        }

        creatingSection = false;
    }

    void InstantiateInitialSection()
    {
        secNum = Random.Range(0, sectionPrefabs.Length);
        GameObject newSection = GetFromPool(secNum);
        newSection.transform.position = new Vector3(0, 0, zPos);
        newSection.transform.SetParent(MAP.transform, false);
        newSection.SetActive(true);

        activeSections.Enqueue(newSection);
    }

    public void EnterMine()
    {
        Debug.Log("Generate Level entered mine");
        Queue<GameObject> newActive = new Queue<GameObject>();
        GameObject mineEntry = null;

        //experimental
        while (activeSections.Count > 0)
        {
            GameObject section = activeSections.Dequeue();
            Chunk chunkData = section.GetComponent<Chunk>();
            if (chunkData == null) continue;

            if (chunkData.chunkNum != mineEntryIndex)
                ReturnToPool(section);
            else
                mineEntry = section;
        }
        if (mineEntry != null)
            newActive.Enqueue(mineEntry);
        activeSections = newActive;
    
 
    }

    public void ExitMine()
    {
        zPos = Mathf.RoundToInt(player.transform.position.z) + resumeAhead;
        Debug.Log($"ExitMine: player.z = {player.transform.position.z}, zPos set to {zPos}");
        creatingSection = true;
        StartCoroutine(ResumeGenerationNextFrame());
    }

    IEnumerator ResumeGenerationNextFrame()
    {
        yield return null; // wait 1 frame
        creatingSection = false;
    }

    // --- Pool management ---
    GameObject GetFromPool(int prefabIndex)
    {
        if (sectionPools[prefabIndex].Count > 0)
            return sectionPools[prefabIndex].Dequeue();
        else
        {
            // Instantiate a new one if pool empty
            GameObject obj = Instantiate(sectionPrefabs[prefabIndex]);
            obj.SetActive(false);
            obj.GetComponent<Chunk>().chunkNum = prefabIndex;
            return obj;
        }
    }

    void ReturnToPool(GameObject section)
    {
        section.SetActive(false);
        section.transform.SetParent(null); // detach from MAP
        Chunk chunkData = section.GetComponent<Chunk>();
        if (chunkData != null)
            sectionPools[chunkData.chunkNum].Enqueue(section);
        else
            Destroy(section); // safety fallback
    }

    public void ResetState()
    {
        Debug.Log("GenerateLevel Reset");
        // Clear active sections
        while (activeSections.Count > 0)
        {
            GameObject section = activeSections.Dequeue();
            ReturnToPool(section);
        }

        // 2. Reset counters/flags
        zPos = 200;  // same as Start()
        generatedSections = 0;
        creatingSection = false;
        disableMinefall = false;

        // 4. Respawn initial sections
        MAP.transform.position = Vector3.zero;   // reset map root

        for (int i = 0; i < 5; i++)
            InstantiateInitialSection();
    }


}*/








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