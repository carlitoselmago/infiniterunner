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

    private LevelData levelData;
    private bool isActive = false;

    private readonly float[] laneX = { -2.84f, 0.12f, 2.96f };

    [Header("Pooling")]
    public int poolSize = 200; // max number of notes
    private int nextNoteIndex = 0;
    private List<GameObject> spawnedNotes = new List<GameObject>();
    private Queue<GameObject> notePoolOriginal = new Queue<GameObject>();
    private Queue<GameObject> notePoolAlternate = new Queue<GameObject>();
    private GameObject notePrefabOriginal;
    private bool usingAlternatePrefab = false;

    [Header("Coins lane shift control")]
    public float beatsPerCluster = 6f;       // duration of each shift cluster
    public float beatsBetweenChecks = 8f;    // check for new shift every X beats
    public float shiftChance = 0.15f;        // chance of triggering a shift cluster
    private bool inShiftCluster = false;
    private float shiftClusterEndTime = 0f;
    private int currentLaneShift = 0;
    private float nextShiftCheckTime = 0f;

    // Alternate mode control (barrels)
    private int currentBurstLane = 1;
    private float nextLaneSwitchTime = 0f;
    private float beatsPerBurst = 6f; // switch lanes every X beats
    private float beatDuration = 0.5f; // will be set from BPM


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
        if (levelData != null && levelData.bpm > 0)
            beatDuration = 60f / levelData.bpm;
    }

    IEnumerator StartModernTimes()
    {
        if (musicSource == null) yield break;

        musicSource.time = 0f;
        musicSource.Play();

        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeModern", 1f, 1f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 3f, 0f));
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 1f, 0f));
        yield return StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 1f, 0.25f));

        PlayerMove.isOnModernTimes = true;
        yield break;
    }

    void Update()
    {
        if (!isActive || levelData == null) return;
        if (musicSource == null) return;

        // Stop controller if music stops
        if (isActive && musicSource.clip != null && musicSource.time > 0.05f)
        {
            if (PlayerMove.isDead || musicSource.time >= musicSource.clip.length - 0.1f)
            {
                PlayerMove.isOnModernTimes = false;
                StartCoroutine(HandleMusicStop());
                return;
            }
        }

        // Spawn notes
        if (nextNoteIndex < levelData.notes.Length)
        {
            float songTime = musicSource.time;
            Note currentNote = levelData.notes[nextNoteIndex];

            // spawn exactly on sync with note time
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
        float[] alternateOnTimes = { 13f, 31f, 56f, 77f };
        float[] alternateOffTimes = { 18f, 36f, 58f };

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
        }
        else if (!shouldUseAlternate && usingAlternatePrefab)
        {
            notePrefab = notePrefabOriginal;
            usingAlternatePrefab = false;
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
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 1f, 0.7f));

        isActive = false;
        gameObject.SetActive(false);
    }

    void SpawnNote(Note note)
    {

        if (note == null) return;
        if (note.lane < 1 || note.lane > laneX.Length) return;

        int laneToUse = note.lane;
        float songTime = musicSource.time;

        if (usingAlternatePrefab)
        {
            // --- Alternate Mode ---
            if (songTime >= nextLaneSwitchTime)
            {
                float burstDuration = beatDuration * beatsPerBurst;
                nextLaneSwitchTime = songTime + burstDuration;

                int newLane = currentBurstLane;
                while (newLane == currentBurstLane)
                    newLane = Random.Range(1, laneX.Length + 1);
                currentBurstLane = newLane;
            }

            // Overlap variation
            if (Random.value < 0.25f)
            {
                int adjLane = Mathf.Clamp(currentBurstLane + (Random.value < 0.5f ? -1 : 1), 1, laneX.Length);
                laneToUse = adjLane;
            }
            else
                laneToUse = currentBurstLane;

            // Density filter
            if (Random.value < 0.4f) return;
        }
        else
        {
            // --- Original Mode with rhythmic clusters ---

            if (songTime >= nextShiftCheckTime)
            {
                nextShiftCheckTime = songTime + beatDuration * beatsBetweenChecks;

                if (!inShiftCluster && Random.value < shiftChance)
                {
                    inShiftCluster = true;

                    // Pick a shift direction
                    currentLaneShift = (Random.value < 0.5f) ? -1 : 1;
                    shiftClusterEndTime = songTime + beatDuration * beatsPerCluster;

                    //Debug.Log($"[{songTime:F1}s] Starting shift cluster ({currentLaneShift:+#;-#})");
                }
            }

            // End the cluster when time passes
            if (inShiftCluster && songTime >= shiftClusterEndTime)
            {
                inShiftCluster = false;
                currentLaneShift = 0;
            }

            // Apply shift if inside an active cluster
            if (inShiftCluster)
                laneToUse = Mathf.Clamp(note.lane + currentLaneShift, 1, laneX.Length);
        }

        GameObject obj = GetNoteFromPool();
        if (obj == null) return;


        float lanePosX = laneX[laneToUse - 1];
        obj.transform.position = new Vector3(lanePosX, 12f, spawnDistance);
        spawnedNotes.Add(obj);
    }

    GameObject GetNoteFromPool()
    {
        var currentPool = usingAlternatePrefab ? notePoolAlternate : notePoolOriginal;
        GameObject obj;

        if (currentPool.Count == 0)
        {
            obj = Instantiate(usingAlternatePrefab ? alternateNotePrefab : notePrefabOriginal);
            if (noteParent != null) obj.transform.SetParent(noteParent);
            else if (mapParent != null) obj.transform.SetParent(mapParent);
            return obj;
        }
        else
            obj = currentPool.Dequeue();

        SetActiveRecursively(obj, true);
        return obj;
    }

    void SetActiveRecursively(GameObject obj, bool state)
    {
        if (obj == null) return;

        obj.SetActive(state);

        foreach (Transform child in obj.transform)
            SetActiveRecursively(child.gameObject, state);
    }


    void CullNotesBehindPlayer()
    {
        for (int i = spawnedNotes.Count - 1; i >= 0; i--)
        {
            GameObject note = spawnedNotes[i];
            if (note == null) continue;

            if (note.activeSelf && note.transform.position.z < -80f)
            {
                SetActiveRecursively(note, false);

                // Return to proper pool
                if (usingAlternatePrefab || note.CompareTag("obstacle"))
                    notePoolAlternate.Enqueue(note);
                else
                    notePoolOriginal.Enqueue(note);

                spawnedNotes.RemoveAt(i);
            }
        }
    }


    public void ResetState()
    {
        nextNoteIndex = 0;

        // --- Disable and properly return all spawned notes ---
        for (int i = spawnedNotes.Count - 1; i >= 0; i--)
        {
            var note = spawnedNotes[i];
            if (note != null)
            {
                SetActiveRecursively(note, false);

                // Use a tag or a stored reference to know which pool to return to
                if (note.CompareTag("obstacle") || note.name.Contains("Alternate"))
                    notePoolAlternate.Enqueue(note);
                else
                    notePoolOriginal.Enqueue(note);
            }
            spawnedNotes.RemoveAt(i);
        }

        // --- Rebuild pools to ensure they have poolSize items ---
        RebuildPool(notePoolOriginal, notePrefabOriginal);
        RebuildPool(notePoolAlternate, alternateNotePrefab);

        // --- Stop music ---
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();

        // --- Reset flags and prefab ---
        notePrefab = notePrefabOriginal;
        usingAlternatePrefab = false;
        inShiftCluster = false;
        currentLaneShift = 0;
        currentBurstLane = 1;
        nextLaneSwitchTime = 0f;
        nextShiftCheckTime = 0f;
        shiftClusterEndTime = 0f;

        // --- Reset game state ---
        isActive = false;
        PlayerMove.isOnModernTimes = false;

        // --- Disable controller ---
        gameObject.SetActive(false);
    }


    void RebuildPool(Queue<GameObject> pool, GameObject prefab)
    {
        // Disable all existing objects and clear the queue
        foreach (var note in pool)
        {
            if (note != null)
                SetActiveRecursively(note, false);
        }
        pool.Clear();

        // Instantiate new objects to fill the pool
        while (pool.Count < poolSize)
        {
            GameObject obj = Instantiate(prefab);
            SetActiveRecursively(obj, false);
            if (noteParent != null) obj.transform.SetParent(noteParent);
            else if (mapParent != null) obj.transform.SetParent(mapParent);
            pool.Enqueue(obj);
        }
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
        //Debug.Log($"Loaded {levelData.notes.Length} notes, bpm {levelData.bpm}");
    }
}
