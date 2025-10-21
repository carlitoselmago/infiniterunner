using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicEventController : MonoBehaviour, IResettable
{
    [Header("References")]
    public AudioSource musicSource;
    public Transform mapParent;
    public GameObject notePrefab; // default prefab
    public GameObject alternateNotePrefab;
    public Transform noteParent;
    public AudioMixer audioMixer;

    [Header("Level data")]
    public string jsonFileName = "modern-times-level";
    public bool useResourcesFolder = false;

    [Header("Note motion")]
    public float fallDuration = 1.5f;
    public float spawnDistance = -30f;

    [Header("Pooling")]
    public int poolSize = 200; // max number of notes
    private Queue<GameObject> notePool = new Queue<GameObject>();

    private LevelData levelData;
    private int nextNoteIndex = 0;
    private bool isActive = false;

    private readonly float[] laneX = { -5.96f, -2.84f, 0.12f };

    private List<GameObject> spawnedNotes = new List<GameObject>();
    private Queue<GameObject> notePoolOriginal = new Queue<GameObject>();
    private Queue<GameObject> notePoolAlternate = new Queue<GameObject>();
    private GameObject notePrefabOriginal;
    private bool usingAlternatePrefab = false;

    void InitializePools()
    {
        InitializeSpecificPool(notePrefabOriginal, notePoolOriginal);
        InitializeSpecificPool(alternateNotePrefab, notePoolAlternate);
    }

    void InitializeSpecificPool(GameObject prefab, Queue<GameObject> pool)
    {
        pool.Clear();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            if (noteParent != null) obj.transform.SetParent(noteParent);
            else if (mapParent != null) obj.transform.SetParent(mapParent);
            pool.Enqueue(obj);
        }
    }

    void Awake()
    {
        notePrefabOriginal = notePrefab;
    }

    void Start()
    {
        LoadLevelData();
    }

    void OnEnable()
    {
        isActive = true;
        nextNoteIndex = 0;
        StartCoroutine(StartModernTimes());
        usingAlternatePrefab = false;
        InitializePools();
    }

    IEnumerator StartModernTimes()
    {
        if (musicSource == null) yield break;

        musicSource.time = 0f;
        musicSource.Play();

        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeModern", 1f, 1f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 3f, 0f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 1f, 0f));
        yield return StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 1f, 0.5f));

        PlayerMove.isOnModernTimes = true;
        yield break;
    }


    void Update()
    {
        if (!isActive || levelData == null) return;
        if (musicSource == null) return;

        // Stop controller if music stops
        if (!musicSource.isPlaying || PlayerMove.isDead)
        {
            PlayerMove.isOnModernTimes = false;
            StartCoroutine(HandleMusicStop());
            return;
        }

        // Spawn notes
        if (nextNoteIndex < levelData.notes.Length)
        {
            float songTime = musicSource.time;
            Note currentNote = levelData.notes[nextNoteIndex];
            if (songTime >= currentNote.time)
            {
                SpawnNote(currentNote);
                nextNoteIndex++;
            }
        }

        CullNotesBehindPlayer();

        // Update prefab timer only if object is active
        HandlePrefabSwapTimer();
    }

    void HandlePrefabSwapTimer()
    {
        if (!isActive || musicSource == null) return;

        float songTime = musicSource.time;

        // Define transition points in seconds
        float[] alternateOnTimes = { 12f, 29f, 51f, 72f };
        float[] alternateOffTimes = { 17f, 34f, 56f };

        // Determine if we should be in alternate mode
        bool shouldUseAlternate = false;
        for (int i = 0; i < alternateOnTimes.Length; i++)
        {
            float on = alternateOnTimes[i];
            float off = (i < alternateOffTimes.Length) ? alternateOffTimes[i] : float.MaxValue;

            if (songTime >= on && songTime < off)
            {
                shouldUseAlternate = true;
                break;
            }
        }

        // Apply the state only when it changes
        if (shouldUseAlternate && !usingAlternatePrefab)
        {
            notePrefab = alternateNotePrefab;
            usingAlternatePrefab = true;
            Debug.Log($"[{songTime:F1}s] → Switched to ALTERNATE prefab");
        }
        else if (!shouldUseAlternate && usingAlternatePrefab)
        {
            notePrefab = notePrefabOriginal;
            usingAlternatePrefab = false;
            Debug.Log($"[{songTime:F1}s] → Switched to NORMAL prefab");
        }
    }

    private IEnumerator HandleMusicStop()
    {
        // Fade out Modern Times
        yield return StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeModern", 0.5f, 0f));

        // Wait briefly for the fade to complete
        yield return new WaitForSeconds(0.2f);

        // Stop music
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();

        // Restore other channels
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 1f, 1f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 1f, 1f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 1f, 1f));

        isActive = false;
        gameObject.SetActive(false);
    }

    void SpawnNote(Note note)
    {
        if (note == null) return;
        if (note.lane < 1 || note.lane > laneX.Length) return; // bounds check

        int laneToUse = note.lane;

        if (usingAlternatePrefab)
        {
            // Alternate mode rules:
            //  - Lane 3: stay in lane 3
            //  - Lane 2: randomly pick lane 2 or lane 1 (if lane 1 exists)
            //  - Lane 1+: stay on current lane safely
            if (note.lane == 2 && laneX.Length >= 3)
                laneToUse = Random.value < 0.5f ? 1 : 2;
            if (Random.value < 0.65f) return; // experimental: reduce number of alternate spawns
        }

        GameObject obj = GetNoteFromPool();
        if (obj == null) return;

        // Convert from 1-based to 0-based index
        float lanePosX = laneX[laneToUse - 1];
        obj.transform.position = new Vector3(lanePosX, 12f, spawnDistance);
        obj.SetActive(true);
        spawnedNotes.Add(obj);
    }

    GameObject GetNoteFromPool()
    {
        var currentPool = usingAlternatePrefab ? notePoolAlternate : notePoolOriginal;

        if (currentPool.Count == 0)
        {
            GameObject obj = Instantiate(usingAlternatePrefab ? alternateNotePrefab : notePrefabOriginal);
            if (noteParent != null) obj.transform.SetParent(noteParent);
            else if (mapParent != null) obj.transform.SetParent(mapParent);
            return obj;
        }

        return currentPool.Dequeue();
    }


    void CullNotesBehindPlayer()
    {
        foreach (var note in spawnedNotes)
        {
            if (note == null) continue;
            if (note.activeSelf && note.transform.position.z < -80f)
                note.SetActive(false); // returns to pool
        }
    }

    public void ResetState()
    {
        nextNoteIndex = 0;

        foreach (var note in spawnedNotes)
            if (note != null) note.SetActive(false);

        spawnedNotes.Clear();

        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();

        notePrefab = notePrefabOriginal;
        usingAlternatePrefab = false;

        isActive = false;
        PlayerMove.isOnModernTimes = false;
        gameObject.SetActive(false);
    }


    void LoadLevelData()
    {
        string json = null;
        if (useResourcesFolder)
        {
            TextAsset ta = Resources.Load<TextAsset>("Levels/" + jsonFileName);
            if (ta == null)
            {
                Debug.LogError("JSON not found in Resources/Levels/" + jsonFileName);
                return;
            }
            json = ta.text;
        }
        else
        {
            string path = Path.Combine(Application.dataPath, "Levels", jsonFileName + ".json");
            if (!File.Exists(path))
            {
                Debug.LogError("JSON not found at " + path);
                return;
            }
            json = File.ReadAllText(path);
        }

        levelData = JsonUtility.FromJson<LevelData>(json);
        Debug.Log($"Loaded {levelData.notes.Length} notes, bpm {levelData.bpm}");
    }
}
