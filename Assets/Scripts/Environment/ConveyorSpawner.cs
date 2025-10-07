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


/// <summary>
/// Handles motion of a pooled conveyor object and recycles it when culled
/// </summary>
public class ConveyorItem : MonoBehaviour
{
    private ConveyorSpawner spawner;
    private float speed;
    private float slopeHeight;
    private float slopeLength;
    private float beltLength;
    private Transform cam;
    private float maxDistance;

    private float distanceTravelled;

    public void Init(ConveyorSpawner spawner, float speed, float slopeHeight, float slopeLength, float beltLength, Transform cam, float maxDistance)
    {
        this.spawner = spawner;
        this.speed = speed;
        this.slopeHeight = slopeHeight;
        this.slopeLength = slopeLength;
        this.beltLength = beltLength;
        this.cam = cam;
        this.maxDistance = maxDistance;
        distanceTravelled = 0f;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        distanceTravelled += step;

        Vector3 pos = spawner.transform.position;

        // 1. Go up slope
        if (distanceTravelled <= slopeLength)
        {
            float t = distanceTravelled / slopeLength;
            pos += new Vector3(0, t * slopeHeight, -distanceTravelled);
        }
        // 2. Flat belt
        else if (distanceTravelled <= slopeLength + beltLength)
        {
            float flatDist = distanceTravelled - slopeLength;
            pos += new Vector3(0, slopeHeight, -(slopeLength + flatDist));
        }
        // 3. Down slope
        else if (distanceTravelled <= slopeLength + beltLength + slopeLength)
        {
            float downDist = distanceTravelled - (slopeLength + beltLength);
            float t = downDist / slopeLength;
            pos += new Vector3(0, slopeHeight * (1 - t), -(slopeLength + beltLength + downDist));
        }
        else
        {
            // reached end of conveyor
            gameObject.SetActive(false);
            return;
        }

        transform.position = pos;

        // Culling check
        if (cam != null)
        {
            Vector3 toObj = transform.position - cam.position;
            float distance = toObj.magnitude;
            bool isInFront = Vector3.Dot(cam.forward, toObj.normalized) > 0f;

            if (!isInFront || distance > maxDistance)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
