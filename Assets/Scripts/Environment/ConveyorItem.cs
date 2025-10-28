using UnityEngine;

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
    private Vector3 startPos;

    // --- Falling state ---
    private bool falling = false;
    private float fallSpeed = 0f;
    private float gravity = 9.81f;
    private float fallYThreshold = -50f; // despawn when falling too far
    private float spawnTime;

    [Header("Ground Check")]
    public float raycastDistance = 1.5f;
    public LayerMask groundMask;
    private int groundLayer;
    private bool onFlatBelt = false;
    private bool groundedAfterFall = false;

    public void Init(ConveyorSpawner spawner, float speed, float slopeHeight, float slopeLength, float beltLength, Transform cam, float maxDistance)
    {
        this.spawner = spawner;
        this.speed = speed;
        this.slopeHeight = slopeHeight;
        this.slopeLength = slopeLength;
        this.beltLength = beltLength;
        this.cam = cam;
        this.maxDistance = maxDistance;
        this.distanceTravelled = 0f;

        startPos = spawner.transform.position;
        falling = false;
        fallSpeed = 0f;
        onFlatBelt = false;
        groundedAfterFall = false;
        spawnTime = Time.time;
    }

    private void Awake()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        groundMask = 1 << groundLayer; // convert to bitmask
    }

    void Update()
    {
        if (Time.time - spawnTime < 0.5f)
            return;

        if (!falling)
        {
            MoveAlongConveyor();
            if (onFlatBelt)
                CheckForGround();
        }
        else
        {
            ApplyFalling();
            CheckForGroundWhileFalling();
        }

        CullIfTooFar();
    }

    void MoveAlongConveyor()
    {
        if (groundedAfterFall) return;

        float step = speed * Time.deltaTime;
        distanceTravelled += step;

        // 1️⃣ Move along the conveyor direction (assumed -Z)
        transform.position += new Vector3(0, 0, -step);

        // 2️⃣ Handle slope up/down based on how far we are
        if (distanceTravelled <= slopeLength)
        {
            onFlatBelt = false;
            float t = distanceTravelled / slopeLength;
            transform.position = new Vector3(
                transform.position.x,
                startPos.y + t * slopeHeight,
                transform.position.z
            );
        }
        else if (distanceTravelled <= slopeLength + beltLength)
        {
            onFlatBelt = true;
            transform.position = new Vector3(
                transform.position.x,
                startPos.y + slopeHeight,
                transform.position.z
            );
        }
        else if (distanceTravelled <= slopeLength + beltLength + slopeLength)
        {
            onFlatBelt = false;
            float downDist = distanceTravelled - (slopeLength + beltLength);
            float t = downDist / slopeLength;
            transform.position = new Vector3(
                transform.position.x,
                startPos.y + slopeHeight * (1 - t),
                transform.position.z
            );
        }
        else
        {
            // End of conveyor — start falling
            onFlatBelt = false;
            falling = true;
        }
    }


    void CheckForGround()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, raycastDistance, groundMask))
        {
            falling = true;
            fallSpeed = 0f;
            onFlatBelt = false;
        }
    }


    void CheckForGroundWhileFalling()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            falling = false;
            groundedAfterFall = true;
            fallSpeed = 0f;
            // Snap to ground surface
            Vector3 pos = transform.position;
            pos.y = hit.point.y;
            transform.position = pos;
        }
    }

    void ApplyFalling()
    {

        if (groundedAfterFall) return;

        fallSpeed += gravity * Time.deltaTime;
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // --- Despawn after falling far enough ---
        if (transform.position.y < fallYThreshold)
            gameObject.SetActive(false);
    }

    void CullIfTooFar()
    {

        if (cam == null) return;

        Vector3 toObj = transform.position - cam.position;
        float distance = toObj.magnitude;
        bool isInFront = Vector3.Dot(cam.forward, toObj.normalized) > 0f;

        if (!isInFront || distance > maxDistance)
        {
            //Debug.Log($"Culled {name} (InFront={isInFront}, Distance={distance})", this);
            gameObject.SetActive(false);

        }
    }
}


