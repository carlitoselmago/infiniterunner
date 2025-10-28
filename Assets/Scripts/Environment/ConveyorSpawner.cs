using UnityEngine;
using System.Collections.Generic;

public class ConveyorSpawner : MonoBehaviour
{
    [Header("Pooling")]
    public Transform cam;                  // Reference to player camera
    public float spawnIntervalMin = 1f;    // Minimum time between spawns
    public float spawnIntervalMax = 3f;    // Maximum time between spawns

    [Header("Conveyor Settings")]
    public float conveyorSpeed = 5f;       // Units per second
    public float beltLength = 250f;        // Flat section length
    public float slopeHeight = 10f;        // How high the slope goes
    public float slopeLength = 20f;        // Horizontal length of slope

    [Header("Culling")]
    public float maxDistance = 300f;       // Cull when behind or too far ahead
    public float stopDistance = 30f;        // Distance at which spawner stops

    private List<GameObject> pool = new List<GameObject>();
    private float nextSpawnTime = 1f;

    void Awake()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        // Collect all children into pool
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            pool.Add(child.gameObject);
        }
    }

    void Update()
    {
        // Stop spawning if camera/player is close to the start of the conveyor
        float distanceToCam = Vector3.Distance(cam.position, transform.position);
        if (distanceToCam < stopDistance)
            return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomObject();
            nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    void SpawnRandomObject()
    {
        // Get an inactive object from pool
        GameObject obj = pool.Find(o => !o.activeInHierarchy);
        if (obj == null) return; // no free objects

        obj.transform.position = transform.position; // reset to spawner position
        obj.SetActive(true);

        ConveyorItem mover = obj.GetComponent<ConveyorItem>();
        if (mover == null)
            mover = obj.AddComponent<ConveyorItem>();

        mover.Init(this, conveyorSpeed, slopeHeight, slopeLength, beltLength, cam, maxDistance);
    }
}