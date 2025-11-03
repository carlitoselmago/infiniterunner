using UnityEngine;
using System.Collections;

public class RideSkateboard : MonoBehaviour, IResettable
{
    public AudioSource rideSFX;
    public AudioSource skateJumpSFX;
    public AudioSource leaveSkateSFX;
    private PlayerMove player;
    private GameObject MAP;
    private Skateboard triggerManager;
    private Animator playerAnimator;
    private Transform skateboardHolder;
    private bool triggered = false;

    private bool initialized = false;
    private Coroutine leaveRoutine;

    public float flipDuration = 1f; // total time for flip animation
    public float flipHeight = 1f;   // how high it lifts during the flip
    private bool isFlipping = false;

    private BoxCollider box;
    private Rigidbody rb;

    private Vector3 positionOffset = new Vector3(1.537f, 0f, 0.166f);

    /// Call after instantiating. The trigger passes itself so this ride can notify it when destroyed.
    public void Initialize(PlayerMove p, GameObject map, Skateboard trigger, Animator playerAnim = null)
    {
        player = p;
        MAP = map;
        triggerManager = trigger;
        playerAnimator = playerAnim;

        gameObject.transform.localPosition = positionOffset;

        initialized = true;
    }

    void OnEnable()
    {
        ResetState();
    }

    private void Start()
    {
        skateboardHolder = player.transform.Find("skateboard");
        if (skateboardHolder == null)
            Debug.LogError("Skateboard holder not found under player.");
    }

    void Update()
    {
        if (!initialized) return;
        if (!PlayerMove.onSkateboard) return; // do nothing unless in skateboard mode

        // ensure PlayerMove knows about this instance
        if (player != null)
            player.skateboardManager = this;

        // Death handling
        if (PlayerMove.isDead && !triggered)
        {
            triggered = true;
            ExitSkateboard();
            return;
        }
    }

    public void SkateJump()
    {
        if (!isFlipping)
        {
            skateJumpSFX.Play();
            rideSFX.Stop();
            StartCoroutine(DoFlip());
        }
    }

    private IEnumerator DoFlip()
    {
        yield return new WaitForSeconds(0.15f);
        isFlipping = true;

        Vector3 startLocalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < flipDuration)
        {
            float t = elapsed / flipDuration;
            float heightOffset = Mathf.Sin(t * Mathf.PI) * flipHeight;

            // Only modify Z (height) relative to current base
            transform.localPosition = new Vector3(
                startLocalPos.x,
                startLocalPos.y,
                startLocalPos.z + heightOffset
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap back to original local position
        transform.localPosition = startLocalPos;
        leaveSkateSFX.Play();
        rideSFX.Play();
        isFlipping = false;
    }

    public void ExitSkateboard()
    {
        if (player == null) return;
        PlayerMove.onSkateboard = false;

        player.StartCoroutine(player.RaisePlayerBody(-0.35f, 0.4f));
        if (leaveSkateSFX != null)
            leaveSkateSFX.Play();
        if (rideSFX != null)
            rideSFX.Stop();

        if (playerAnimator != null)
            playerAnimator.SetBool("isskating", false);
        else
        {
            // fallback: try to find player's child animator
            var anim = player.GetComponentInChildren<Animator>();
            if (anim != null) anim.SetBool("isskating", false);
        }

        player.moveSpeed = 12f;
        if (player != null) player.skateboardManager = null;

        // stop any previous leave coroutine
        if (leaveRoutine != null) StopCoroutine(leaveRoutine);
        leaveRoutine = StartCoroutine(LeaveAndDestroy());
    }

    private IEnumerator LeaveAndDestroy()
    {
        // Unparent, leave on MAP and apply inertia
        box = gameObject.AddComponent<BoxCollider>();
        rb = gameObject.AddComponent<Rigidbody>();

        if (MAP != null) transform.SetParent(MAP.transform, true);
        float launchForce = PlayerMove.currentSpeed * 4.5f;
        Vector3 launchDir = player.transform.forward + Vector3.up * 0.3f;
        rb.AddForce(player.transform.up * 5f, ForceMode.Impulse);
        rb.AddForce(launchDir.normalized * launchForce, ForceMode.Impulse);
        rb.AddTorque(Vector3.right * Random.Range(5f, 10f), ForceMode.Impulse);

        // If something else was left under skateboardHolder, make sure it's not active
        if (skateboardHolder != null)
        {
            for (int i = skateboardHolder.childCount - 1; i >= 0; i--)
            {
                var child = skateboardHolder.GetChild(i);
                if (child != null)
                    child.gameObject.SetActive(false);
            }
        }

        // small visual delay so the object remains visible for a moment
        yield return new WaitForSeconds(12f);
        // Before destroying, notify trigger manager so it doesn't keep a stale reference
        if (triggerManager != null)
            triggerManager.NotifyRideDestroyed(gameObject);

        Destroy(gameObject);
    }

    // Function to clear the skateboard if entering a forklift or minecart
    public void ClearSkateboard()
    {
        PlayerMove.onSkateboard = false;
        if (player != null)
            player.skateboardManager = null;
        if (playerAnimator != null)
            playerAnimator.SetBool("isskating", false);
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (triggerManager != null)
            triggerManager.NotifyRideDestroyed(gameObject);
        if (transform.parent != null && transform.parent.name == "skateboard")
            transform.SetParent(null);
    }

    void OnDestroy()
    {
        if (triggerManager != null)
            triggerManager.NotifyRideDestroyed(gameObject);
    }

    public void ResetState()
    {
        triggered = false;
    }
}