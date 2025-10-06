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

        // Prevent more than one active mine section at a time
        if (secNum == mineEntryIndex && minePresent)
        {
            // Force reroll until it's not the mine
            int tries = 0;
            while (secNum == mineEntryIndex && tries < 10)
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

        // Track mine presence
        if (chunkData != null && chunkData.chunkNum == mineEntryIndex)
        {
            minePresent = true;
            protectMineSection = true;
        }

        activeSections.Enqueue(newSection);

        // Dequeue and return to pool - experimental block (fly protection)
        if(activeSections.Count > 0)
        {
            GameObject oldest = activeSections.Peek(); // look at first in queue
            Chunk oldestChunk = oldest.GetComponent<Chunk>();
            if(oldestChunk != null)
            {
                float sectionEndZ = oldest.transform.position.z + oldestChunk.chunkLength + oldestChunk.cullBuffer;
                int mapFront = -Mathf.RoundToInt(MAP.transform.position.z);
                if(mapFront > sectionEndZ && activeSections.Count > 8)
                {
                    oldest = activeSections.Dequeue();
                    ReturnToPool(oldest);
                    if(protectMineSection && oldestChunk.chunkNum == mineEntryIndex)
                    {
                        minePresent = false;
                        protectMineSection = false;
                    }
                }
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
            //int num = (chunkData != null) ? chunkData.chunkNum : -999;
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

        StartCoroutine(ResumeGenerationStaggered(4, 0.2f));
    }

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
        if (!sectionPools.ContainsKey(prefabIndex))
        {
            Debug.LogError($"No pool found for prefab index {prefabIndex}! templatesparent has {sectionPrefabs.Length} prefabs.");
            prefabIndex = 0; // fallback to first prefab to avoid crash
        }

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