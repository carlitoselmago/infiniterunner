using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

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
    public float maxHealth = 100f;
    public float currentHealth;
    public float damagePerSecond = 10f;
    public Image fillImage;
    public Image background;
    public CanvasGroup sliderCanvasGroup;
    private RectTransform healthBarRect;
    private Tweener shakeTween;
    private bool isShaking = false;
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

    void OnEnable()
    {
        ResetState();
        InitHealthUI();
    }

    private void InitHealthUI()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (fillImage != null) fillImage.color = Color.white;
        if (background != null) background.color = Color.white;
        if (sliderCanvasGroup != null) sliderCanvasGroup.alpha = 0.5f;

        if (smoke != null) smoke.SetActive(false);
        StopHealthShake();
    }

    private void Start()
    {
        forkliftHolder = player.transform.Find("forklift");
        if (forkliftHolder == null)
            Debug.LogError("Forklift holder not found under player! Please create a child named 'forklift'.");
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
            canvas.SetActive(false);
            Debug.Log("Explosion");

            if (explosionPrefab != null)
            {
                // spawn explosion at forklift position
                GameObject expl = Instantiate(explosionPrefab, transform.position, transform.rotation);

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
        if (!canvas.activeSelf && !PlayerMove.isDead)
            canvas.SetActive(true);

        currentHealth -= damagePerSecond * dt;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;

        // Fill color (white -> red)
        if (fillImage != null)
        {
            float tFill = 1f - (currentHealth / maxHealth); // 0 at full, 1 at zero
            fillImage.color = Color.Lerp(Color.white, Color.red, tFill);
        }

        // Canvas alpha
        if (sliderCanvasGroup != null)
        {
            sliderCanvasGroup.alpha = Mathf.Lerp(0.5f, 1f, 1f - (currentHealth / maxHealth));
        }

        // LOW HEALTH logic (<= 50). Update background color every tick while low.
        if (currentHealth <= 50f)
        {
            if (!lowHealth)
            {
                lowHealth = true;
                if (smoke != null) smoke.SetActive(true);

                // start a continuous shake
                StartHealthShake();
            }

            if (background != null)
            {
                // Map currentHealth 50 -> 0 into t 0 -> 1
                float tBg = Mathf.Clamp01(1f - (currentHealth / 50f));
                background.color = Color.Lerp(Color.white, Color.red, tBg);
            }
        }
        else
        {
            // If you recover above 50, stop shake and reset background
            if (lowHealth)
            {
                lowHealth = false;
                if (smoke != null) smoke.SetActive(false);
                StopHealthShake();

                if (background != null) background.color = Color.white;
            }
        }

        // Death
        if (!forkliftDestroyed && currentHealth <= 0f && !PlayerMove.isDead)
        {
            PlayerMove.isDead = true;
            forkliftDestroyed = true;
            canvas.SetActive(false);
            StopHealthShake();
            player.DieOnForklift();
        }
    }

    private void StartHealthShake()
    {
        if (isShaking) return;

        // Prefer shaking the healthBar RectTransform, not the whole Canvas
        if (healthBarRect == null && healthBar != null)
            healthBarRect = healthBar.GetComponent<RectTransform>();

        if (healthBarRect == null) return;

        // Kill any previous tween, then start a looping anchor-pos shake
        healthBarRect.DOKill();
        shakeTween = healthBarRect.DOShakeAnchorPos(
            duration: 1.0f,                     // how long one cycle is (we will loop)
            strength: new Vector2(12f, 0f),     // shake strength (x, y)
            vibrato: 20,                        // how many strikes
            randomness: 90f,                    // randomness
            snapping: false,
            fadeOut: true
        )
        .SetLoops(-1, LoopType.Restart);

        isShaking = true;
    }

    private void StopHealthShake()
    {
        if (!isShaking) return;

        if (shakeTween != null) shakeTween.Kill();
        if (healthBarRect != null)
        {
            healthBarRect.anchoredPosition = Vector2.zero; // reset pos cleanly
        }
        isShaking = false;
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
