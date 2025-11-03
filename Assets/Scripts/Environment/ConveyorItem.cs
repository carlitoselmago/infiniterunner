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
    private float gravity = 9.81f;
    private float fallYThreshold = -50f; // despawn if falling too far
    private float spawnTime;

    [Header("Ground Check")]
    public float raycastDistance = 1.5f;
    public LayerMask groundMask;
    private int groundLayer;
    private bool onFlatBelt = false;
    private bool groundedAfterFall = false;

    private Rigidbody rb;

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
        onFlatBelt = false;
        groundedAfterFall = false;
        spawnTime = Time.time;
    }

    private void Awake()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        groundMask = 1 << groundLayer; // convert to bitmask
    }

    private void OnEnable()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Time.time - spawnTime < 0.5f) return;

        if (!falling)
        {
            MoveAlongConveyor();
            if (onFlatBelt)
                CheckForGround();
        }

        CullIfTooFar();
    }

    void MoveAlongConveyor()
    {
        if (groundedAfterFall) return;

        float step = speed * Time.deltaTime;
        distanceTravelled += step;

        // 1️⃣ Move along the conveyor direction (-Z)
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
            StartFalling();
    }


    void CheckForGround()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, raycastDistance, groundMask))
            StartFalling();
    }

    void StartFalling()
    {
        if (falling) return;
        falling = true;
        onFlatBelt = false;

        rb.isKinematic = false;
        rb.velocity = new Vector3(0, 0, -speed * 0.5f);
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