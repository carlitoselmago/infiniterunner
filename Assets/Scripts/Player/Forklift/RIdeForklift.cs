using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class RideForklift : MonoBehaviour, IResettable
{
    private PlayerMove player;
    private GameObject MAP;
    private Forklift triggerManager;
    private Animator playerAnimator;
    private Transform forkliftHolder;
    private bool triggered = false;

    [Header("Repair")]
    public AudioClip repairSFX;
    private AudioSource repairAudio;
    public Light[] warningLights;
    private float pulseTimer = 0f;
    private bool isRepairPulse = false;

    [Header("Explosion")]
    public GameObject explosionPrefab;

    [Header("Smoke FX")]
    private ParticleSystem smokeParticles;
    private ParticleSystem.EmissionModule smokeEmission;
    private float maxSmokeRate = 4000f;

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
    public bool lowHealth = false;
    public static bool criticalHealth = false;
    public static bool forkliftDestroyed = false;
    public static bool isRepairing = false;
    public GameObject canvas;
    public GameObject instructionCanvas;
    public GameObject smoke;
    public Slider healthBar;
    private RectTransform exitTextRect;
    private Tweener exitTextTween;


    private bool initialized = false;
    private Coroutine leaveRoutine;

    /// Call after instantiating. The trigger passes itself so this ride can notify it when destroyed.
    public void Initialize(PlayerMove p, GameObject map, Forklift trigger, Animator playerAnim = null)
    {
        player = p;
        MAP = map;
        triggerManager = trigger;
        playerAnimator = playerAnim;

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

        if (smoke != null) smoke.SetActive(true);
        if (instructionCanvas != null) instructionCanvas.SetActive(true);
        StopHealthShake();
    }

    private void Start()
    {
        forkliftHolder = player.transform.Find("forklift");
        //if (forkliftHolder == null)
            //Debug.LogError("Forklift holder not found under player! Please create a child named 'forklift'.");

        if (repairSFX != null)
        {
            repairAudio = gameObject.AddComponent<AudioSource>();
            repairAudio.clip = repairSFX;
            repairAudio.loop = true;
            repairAudio.playOnAwake = false;
            repairAudio.volume = 0.6f;
        }

        if (smoke != null)
        {
            smokeParticles = smoke.GetComponent<ParticleSystem>();
            if (smokeParticles != null)
            {
                smokeEmission = smokeParticles.emission;
                smokeEmission.rateOverTime = 0f;
                smokeParticles.Play();
            }
        }

        if (instructionCanvas != null)
        {
            var textObj = instructionCanvas.transform.Find("text-sortir");
            if (textObj != null)
                exitTextRect = textObj.GetComponent<RectTransform>();
        }
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
            instructionCanvas.SetActive(false);
            Explode();
            return;
        }

        // --- Gradual health recovery when holding Down Arrow ---
        if (Input.GetKey(KeyCode.DownArrow) && !forkliftDestroyed && !PlayerMove.isDead && PlayerMove.onForklift)
        {
            isRepairing = true;
            float recoverSpeed = 5f; // health points per second
            currentHealth += recoverSpeed * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            playerAnimator.SetBool("isrepairing", true);

            if (healthBar != null)
                healthBar.value = currentHealth;

            if (fillImage != null)
            {
                float tFill = 1f - (currentHealth / maxHealth);
                fillImage.color = Color.Lerp(Color.white, Color.red, tFill);
            }

            if (sliderCanvasGroup != null)
                sliderCanvasGroup.alpha = Mathf.Lerp(0.5f, 1f, 1f - (currentHealth / maxHealth));

            // Play repair sound
            if (repairAudio != null && !repairAudio.isPlaying)
                repairAudio.Play();

            if (!isRepairPulse)
            {
                isRepairPulse = true;
                pulseTimer = 0f;
                SetWarningLightsColor(Color.green);
            }

            PulseWarningLights(Color.green, Color.green, 10f);

            // Stop smoke and shake if health rises above 50
            if (lowHealth && currentHealth > 50f)
            {
                lowHealth = false;
                StopHealthShake();
                StopTextShake();
                if (background != null) background.color = Color.white;
            }
        }
        else
        {
            // Stop when key released
            if (repairAudio != null && repairAudio.isPlaying)
                repairAudio.Stop();

            // Return to normal red flashing
            if (isRepairPulse)
            {
                pulseTimer = 0f;
                isRepairPulse = false;
            }
            SetWarningLightsColor(Color.red);

            if (isRepairing)
            {
                isRepairing = false;
                playerAnimator.SetBool("isrepairing", false);
            }
        }

        if (!isRepairing && warningLights != null)
            PulseWarningLights(Color.black, Color.red, 2f);

        UpdateSmokeEffect();
    }

    private void SetWarningLightsColor(Color color)
    {
        if (warningLights == null) return;
        foreach (var light in warningLights)
            if (light != null) light.color = color;
    }

    private void PulseWarningLights(Color colorA, Color colorB, float speed = 3f)
    {
        if (warningLights == null) return;
        pulseTimer += Time.deltaTime * speed;
        float t = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        Color c = Color.Lerp(colorA, colorB, t);
        foreach (var light in warningLights)
            if (light != null) light.color = c;
    }

    private void UpdateSmokeEffect()
    {
        if (smokeParticles == null) return;

        // Map health (100 → 0) to emission (0 → maxSmokeRate)
        float t = 1f - (currentHealth / maxHealth);
        float targetRate = Mathf.Lerp(0f, maxSmokeRate, t * t); // quadratic easing for smoother start

        smokeEmission.rateOverTime = targetRate;

        var main = smokeParticles.main;
        Color baseColor = Color.Lerp(new Color(0.7f, 0.7f, 0.7f, 0.3f), new Color(0.3f, 0.3f, 0.3f, 0.6f), t);
        main.startColor = baseColor;
    }

    public void ExitForklift()
    {
        if (player == null) return;
        PlayerMove.onForklift = false;

        canvas.SetActive(false);
        instructionCanvas.SetActive(false);

        player.StartCoroutine(player.RaisePlayerBody(-0.35f, 0.6f));

        if (playerAnimator != null)
            playerAnimator.SetBool("isdrivingminecart", false);
        else
        {
            // fallback: try to find player's child animator
            var anim = player.GetComponentInChildren<Animator>();
            if (anim != null) anim.SetBool("isdrivingminecart", false);
        }

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
            sliderCanvasGroup.alpha = Mathf.Lerp(0.5f, 1f, 1f - (currentHealth / maxHealth));

        // LOW HEALTH logic (<= 50). Update background color every tick while low.
        if (currentHealth <= 50f)
        {
            if (!lowHealth)
            {
                lowHealth = true;
                StartHealthShake();
            }

            if (lowHealth && exitTextRect != null)
            {
                if (exitTextTween == null || !exitTextTween.IsActive())
                {
                    exitTextTween = exitTextRect.DORotate(new Vector3(0, 0, 15f), 0.1f, RotateMode.LocalAxisAdd)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.Linear);
                }
            }

            if (background != null)
            {
                // Map currentHealth 50 -> 0 into t 0 -> 1
                float tBg = Mathf.Clamp01(1f - (currentHealth / 50f));
                background.color = Color.Lerp(Color.white, Color.red, tBg);
            }

            if (currentHealth <= 20f)
                criticalHealth = true;
        }

        else
        {
            // If you recover above 50, stop shake and reset background
            if (lowHealth)
            {
                lowHealth = false;
                StopHealthShake();
                StopTextShake();

                if (background != null) background.color = Color.white;
            }
        }

        // Death
        if (!forkliftDestroyed && currentHealth <= 0f)
        {
            forkliftDestroyed = true;
            canvas.SetActive(false);
            instructionCanvas.SetActive(false);
            StopHealthShake();
            Explode();
            player.DieOnForklift();
            return;
        }
    }

    private void Explode()
    {
        if (explosionPrefab != null)
        {
            isRepairing = false;
            smoke.SetActive(false);
            // spawn explosion at forklift position
            GameObject expl = Instantiate(explosionPrefab, transform.position, transform.rotation);

            // enable Explodable script if it’s not already enabled
            var explScript = GetComponent<Explodable>();
            if (explScript != null)
                explScript.enabled = true;
            forkliftDestroyed = true;
            var audioSources = GetComponents<AudioSource>();
            foreach (var source in audioSources)
            {
                if (source.loop)
                    source.Stop();
            }
            //Debug.Log("Forklift exploded!");
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
            healthBarRect.anchoredPosition = Vector2.zero; // reset pos cleanly
        isShaking = false;
    }

    private void StopTextShake()
    {
            if (exitTextTween != null && exitTextTween.IsActive())
            {
                exitTextTween.Kill();
                exitTextRect.localRotation = Quaternion.identity;
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
        yield return new WaitForSeconds(12f);
        // Before destroying, notify trigger manager so it doesn't keep a stale reference
        if (triggerManager != null)
            triggerManager.NotifyRideDestroyed(gameObject);

        Destroy(gameObject);
    }

    void OnDisable()
    {
        // Notify trigger if possible
        if (triggerManager != null)
            triggerManager.NotifyRideDestroyed(gameObject);

        // Extra cleanup safeguard: remove from holder if still attached
        if (transform.parent != null && transform.parent.name == "forklift")
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
        lowHealth = false;
        criticalHealth = false;
        forkliftDestroyed = false;
    }
}