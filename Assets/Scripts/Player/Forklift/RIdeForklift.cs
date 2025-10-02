using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class RideForklift : MonoBehaviour, IResettable
{
    private PlayerMove player;
    private GameObject MAP;
    private Forklift triggerManager;
    private Animator playerAnimator;
    private Transform forkliftHolder;
    private bool triggered = false;

    [Header("Fork Controls")]
    private Transform forkTransform;
    private Rigidbody forkRb;
    public float forkSpeed = 1.5f;
    public float minForkZ = 0f;
    public float maxForkZ = 4.5f;

    [Header("Explosion")]
    public GameObject explosionPrefab;

    [Header("Forklift Endurance")]
    public float maxHealth = 150f;
    public float currentHealth;
    public float damagePerSecond = 10f;
    public static bool lowHealth = false;
    public static bool forkliftDestroyed = false;
    public GameObject canvas;
    public GameObject smoke;
    public Slider healthBar;

    private bool initialized = false;
    private Coroutine leaveRoutine;

    /// Call after instantiating. The trigger passes itself so this ride can notify it when destroyed.
    public void Initialize(PlayerMove p, GameObject map, Forklift trigger, Animator playerAnim = null)
    {
        player = p;
        MAP = map;
        triggerManager = trigger;
        playerAnimator = playerAnim;

        // find fork (VisMast) deep in hierarchy
        forkTransform = GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "VisMast");

        if (forkTransform != null)
        {
            forkRb = forkTransform.GetComponent<Rigidbody>();
            if (forkRb == null)
                forkRb = forkTransform.gameObject.AddComponent<Rigidbody>();
            forkRb.isKinematic = true;
        }

        initialized = true;
    }

    private void Start()
    {
        forkliftHolder = player.transform.Find("forklift");
        if (forkliftHolder == null)
            Debug.LogError("Forklift holder not found under player! Please create a child named 'forklift'.");

        currentHealth = maxHealth;
        if (healthBar != null) healthBar.maxValue = maxHealth;
    }

    void Update()
    {
        if (!initialized) return;
        if (!PlayerMove.onForklift) return; // do nothing unless in forklift mode

        // ensure PlayerMove knows about this instance
        if (player != null)
            player.forkliftManager = this;

        // Death / explosion handling
        if (PlayerMove.isDead && !triggered)
        {
            triggered = true;
            Debug.Log("Explosion");

            if (explosionPrefab != null)
            {
                // spawn explosion at forklift position
                GameObject expl = Instantiate(explosionPrefab, transform.position, transform.rotation);

                canvas.SetActive(false);
                // enable Explodable script if it’s not already enabled
                var explScript = GetComponent<Explodable>();
                if (explScript != null)
                    explScript.enabled = true;
                forkliftDestroyed = true;
            }
            return;
        }

        // Fork movement (local Z)
        /*
        if (forkTransform != null)
        {
            Vector3 pos = forkTransform.localPosition;
            if (Input.GetKey(KeyCode.UpArrow)) pos.z += forkSpeed * Time.deltaTime; // maybe too much - compromise
            else if (Input.GetKey(KeyCode.DownArrow)) pos.z -= forkSpeed * Time.deltaTime;
            pos.z = Mathf.Clamp(pos.z, minForkZ, maxForkZ);
            forkTransform.localPosition = pos;
        }*/
    }

    public void ExitForklift()
    {
        if (player == null) return;

        canvas.SetActive(false);
        smoke.SetActive(false);

        // Lower player visual body
        player.StartCoroutine(player.RaisePlayerBody(-0.35f, 0.6f));

        // toggle animator safely (playerAnimator might be assigned from trigger)
        if (playerAnimator != null)
            playerAnimator.SetBool("isdrivingminecart", false);
        else
        {
            // fallback: try to find player's child animator
            var anim = player.GetComponentInChildren<Animator>();
            if (anim != null) anim.SetBool("isdrivingminecart", false);
        }

        PlayerMove.onForklift = false;
        player.moveSpeed = 12f;
        if (player != null) player.forkliftManager = null;

        // stop any previous leave coroutine
        if (leaveRoutine != null) StopCoroutine(leaveRoutine);
        leaveRoutine = StartCoroutine(LeaveAndDestroy());
    }

    public void ApplyCollisionDamage(float dt)
    {
        if (!canvas.activeSelf)
            canvas.SetActive(true);     // reset to false if no damage is taken?
        currentHealth -= damagePerSecond * dt;
        if (healthBar != null) healthBar.value = currentHealth;

        if (!lowHealth && currentHealth <= 50)
        {
            lowHealth = true;
            smoke.SetActive(true);
        }

        if (!forkliftDestroyed && currentHealth <= 0 && !PlayerMove.isDead)
        {
            PlayerMove.isDead = true;
            forkliftDestroyed = true;
            player.DieOnForklift();
        }
    }

    private IEnumerator LeaveAndDestroy()
    {
        // Unparent and leave on MAP
        if (MAP != null) transform.SetParent(MAP.transform, true);
        // If something else was left under forkliftHolder, make sure it's not active
        if (forkliftHolder != null)
        {
            for (int i = forkliftHolder.childCount - 1; i >= 0; i--)
            {
                var child = forkliftHolder.GetChild(i);
                if (child != null)
                    child.gameObject.SetActive(false);
            }
        }

        // small visual delay so the object remains visible for a moment
        yield return null;/*
        yield return new WaitForSeconds(3f);

        // Before destroying, notify trigger manager so it doesn't keep a stale reference
        if (triggerManager != null)
            triggerManager.NotifyRideDestroyed(gameObject);

        Destroy(gameObject);*/
    }

    void OnDisable()
    {
        // If the ride is disabled (manual disable flow), inform the trigger manager
        if (triggerManager != null)
            triggerManager.NotifyRideDestroyed(gameObject);
    }

    void OnDestroy()
    {
        if (triggerManager != null)
            triggerManager.NotifyRideDestroyed(gameObject);
    }

    public void ResetState()
    {
        triggered = false;
        lowHealth = false;
        forkliftDestroyed = false;
    }
}
