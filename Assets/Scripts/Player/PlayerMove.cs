using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using SuperPivot.Samples;

public class PlayerMove : MonoBehaviour, IResettable
{
    private Vector3 startPosition = new Vector3(0f, -0.35f, -48f);
    public float moveSpeed = 12f;
    public float skateboardSpeed = 20f;
    public static float currentSpeed = 12.0f;
    private float initialmoveSpeed = 0;
    public float horizontalSpeed = 20f;
    private Quaternion startRotation;
    public bool isJumping = false;
    public bool isRolling = false;
    public bool isFlying = false;
    public static bool isOnTheAir = false;
    public bool holding = false;
    public static bool paused = false;
    public static bool onMinecart = false;
    public static bool onForklift = false;
    public static bool onSkateboard = false;
    private bool mainThemeAlreadyPlaying = false;
    public static bool idle = true;
    public static bool isUnderwater = false;
    public static bool isOnModernTimes = false;
    public static bool isInTheSandstorm = false;
    private bool endSequenceStarted = false;

    [Header("Constrains")]
    public bool blockLeft = false;
    public bool blockRight = false;

    public LaneHighlight laneHighlight;

    // raycast
    [Header("Raycast")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public static float rayLength = 1.2f;
    public float exposedRayLength = 1.2f;
    public float raycastHeightOffset = 0.5f;
    public bool isGrounded = false;
    public bool isFalling = false;

    [Header("Health")]
    public static int maxHealth = 5;
    public static int remainingHealth;
    private bool hit = false;
    public static bool isDead = false;
    public bool godmode = false;
    public static bool bonus = false;

    [Header("Bonus Visual Feedback")]
    public Light globalLight;
    public Light bonusSpotlight;
    public GameObject bonusUI;
    public Material godModeMaterial;
    public Material bonusMaterial;

    [Header("Fly coins")]
    public GameObject flycoin;
    public int flycoinsamount = 30;
    public Transform coinContainer;
    private List<GameObject> instantiatedCoins = new List<GameObject>();

    public GameObject godmodevisual;
    public GameObject playerObject;
    public Rigidbody playerBody;        // assign root Rigidbody
    public GameObject startingText;
    public GameObject tutorialText;
    private Animator animator;
    private Animator camAnimator;
    public GameObject mainCam;
    public GameObject rocks;
    public Minecart minecart;
    public RotateAroundPivot sphereScript;

    //forklift
    private float forkliftOffsetX = 0f;
    float leftLane = -10f;
    float rightLane = 10f;
    public RideForklift forkliftManager;
    public RideSkateboard skateboardManager;

    //sfx
    [Header("SFX")]
    public AudioSource HurtSFX;
    public AudioSource crashThud;
    public AudioSource minecartCrashSFX;
    public AudioSource minecartShiftLaneSFX;
    public AudioSource BGM;
    public AudioSource mainTheme;
    public AudioSource pyramidsTheme;
    public AudioSource flyFX;
    public AudioSource cogFactorySFX;
    public AudioSource cogsfarmSFX;
    public AudioSource photosSFX;
    public AudioSource backDoorSFX;
    public AudioSource panopticSFX;
    public AudioSource canyonSFX;
    public AudioSource claxonSFX;
    public AudioSource carCrashSFX;
    public AudioSource cardboard1;
    public AudioSource cardboard2;
    public AudioSource bonusSFX;
    [Header("Collision Sounds")]
    public AudioSource rumbleSFX;
    public AudioClip[] hitClips;
    private float soundInterval = 0.8f;
    private float soundTimer = 0f;

    //pitch shifter for flying timeout
    private float startingPitch = 1.5f;
    private float endingPitch = 1f;
    private float pitchDuration = 0.8f;

    //audio mixer
    public AudioMixer audioMixer;

    public GameObject levelControl;
    public CollectableControl collectableControl;

    // IMPORTANT: main physics collider (non-trigger). Keep this always enabled to avoid sinking.
    public BoxCollider boxCollider;
    public BoxCollider forkliftCollider;

    public HitLogic hitLogic; // assign the child HitLogic in inspector or it will auto-find

    private float targetHeight = 22.0f; // previously 17.0f
    private float startY;
    float fallTimer = 0f;
    private Transform playerStartParent; // To remember original parent
    private Vector3 playerLocalPos;      // Local position relative to parent
    private Quaternion playerLocalRot;   // Local rotation relative to parent

    // Timing
    public static float runStartTime;
    private float jumpStarted;

    public GameObject tutorial2d;
    private float timer;
    private bool alreadyCrossedPanoptic = false;
    public AudioSource coinFX;
    public HurtMask hurtMaskScript;
    public AudioSource minecartObject;
    private bool triggered = false;
    public GameObject endstormText;

    public GameObject MAP;

    public GameObject hearts;
    public GameObject heart;
    public List<GameObject> heartList = new List<GameObject>();

    public string pos = "center";
    private float targetpos = 0f;
    public static bool startedrunning = false;

    private string tutorialcard = "";
    private Dictionary<string, string> tutorialInstructions = new Dictionary<string, string>
    {
        { "crouch", "AJUP-TE!" },
        { "jump", "SALTA!" },
        { "left", "ESQUERRA!" },
        { "right", "DRETA!" },
        { "rocket", "AGAFA EL COET!" }
    };

    public PrintCode printCodeScript;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        camAnimator = mainCam.GetComponent<Animator>();
        if (playerBody == null) playerBody = GetComponent<Rigidbody>();

        // Find main non-trigger collider (boxCollider) if not assigned
        if (boxCollider == null)
        {
            BoxCollider[] all = GetComponentsInChildren<BoxCollider>();
            foreach (var c in all)
                if (!c.isTrigger) { boxCollider = c; break; }
        }

        // Find HitLogic child if not assigned
        if (hitLogic == null)
            hitLogic = GetComponentInChildren<HitLogic>();

        // Ensure hitboxes default state: normal hitbox active, jump hitbox off
        if (hitLogic != null)
            hitLogic.EnableHitbox(HitLogic.HitboxType.Normal);

        startY = transform.position.y;
        startRotation = transform.rotation;
        onMinecart = false;
        BGM.pitch = 1.0f;
        HideAllTutorialCards();
        isDead = false;
        remainingHealth = 0;
        startedrunning = false;
        godmodevisual.SetActive(false);
        initialmoveSpeed = moveSpeed;
        collectableControl = FindObjectOfType<CollectableControl>();

        if (playerObject != null)
        {
            playerStartParent = playerObject.transform.parent;
            playerLocalPos = playerObject.transform.localPosition;
            playerLocalRot = playerObject.transform.localRotation;
        }

        //set hearts based on amount of life
        for (int i = 0; i < maxHealth; i++)
            AddHeart();
    }

    public void AddHeart()
    {
        if (heartList.Count < maxHealth)
        {
            GameObject clonedHeart = Instantiate(heart, Vector3.zero, Quaternion.identity);
            clonedHeart.transform.SetParent(hearts.transform, false);
            clonedHeart.transform.localPosition = new Vector3(heartList.Count * 50, 0, 0);
            Animator heartAnimator = clonedHeart.GetComponent<Animator>();
            heartList.Add(clonedHeart);
            heartAnimator.SetBool("started", true);
            remainingHealth += 1;
            if (!idle)
                Debug.Log("Added Heart. Remaining Health: " + remainingHealth);
        }
        else
            Debug.Log("Cannot add more hearts");
    }

    public void RemoveHeartsInReverseOrder()
    {
        if (remainingHealth < 0) return;
        int lastindex = heartList.Count - 1;
        Destroy(heartList[lastindex]);
        heartList.RemoveAt(lastindex);
    }

    void Update()

    {
        if (paused) return;

        exposedRayLength = rayLength;
        currentSpeed = moveSpeed;
        isOnTheAir = holding;

        if (!isFlying && !isUnderwater && !isDead && !onForklift && !onMinecart)
            moveSpeed = onSkateboard ? skateboardSpeed : initialmoveSpeed;

        UpdateActiveCollider();

        // Quit the game (Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        // Start sequence (Idle)
        if (!startedrunning && idle)
        {
            if (!startingText.activeSelf)
            {
                startingText.GetComponent<Text>().text = "Agafa les eines i toca qualsevol engranatge";
                startingText.SetActive(true);
            }
            timer += Time.deltaTime;
            if (timer >= 2f)
            {
                tutorial2d.transform.Find("touch-cards").gameObject.SetActive(true);
                if (timer >= 7f)
                {
                    tutorial2d.transform.Find("touch-cards").gameObject.SetActive(false);
                    //startingText.SetActive(false);
                    timer = 0f;
                    bool stretchArmsAnimation = Random.value > 0.65f;
                    if (stretchArmsAnimation) animator.SetTrigger("stretch");
                }
            }
        }

        if (startedrunning && !animator.GetBool("isrunning"))
        {
            runStartTime = Time.unscaledTime;   // start counting play time
            animator.SetBool("isrunning", true);
        }

        if (animator.GetBool("isrunning"))
            MAP.transform.Translate(Vector3.back * Time.deltaTime * moveSpeed, Space.World);

        if (playerBody.IsSleeping())
            playerBody.WakeUp();

        if (!onForklift)
        {
            // Left
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (!startedrunning)
                    StartPlay();

                if (tutorialcard == "left") tutorial2d.transform.Find(tutorialcard).gameObject.SetActive(false);
                if (!isFlying)
                {
                    if (blockLeft)
                    {
                        animator.SetTrigger("blockleft");
                        return;
                    }

                    if (pos == "left")
                    {
                        laneHighlight.Flash();
                        animator.SetTrigger("blockleft");
                        return;
                    }

                    if (pos == "center") // Pressing left from center goes to left
                    {
                        pos = "left";
                        if (onMinecart)
                        {
                            minecartShiftLaneSFX.panStereo = -0.7f;
                            minecartShiftLaneSFX.Play();
                        }

                    }
                    else if (pos == "right") // Pressing left when at right goes to center
                    {
                        pos = "center";
                        if (onMinecart)
                        {
                            minecartShiftLaneSFX.panStereo = 0f;
                            minecartShiftLaneSFX.Play();
                        }
                    }
                    printCodeScript.SetCodePrompt("left");
                }
            }

            // Right
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (!startedrunning)
                    StartPlay();

                if (tutorialcard == "right") tutorial2d.transform.Find(tutorialcard).gameObject.SetActive(false);
                if (!isFlying)
                {
                    if (blockRight)
                    {
                        animator.SetTrigger("blockright");
                        return;
                    }

                    if (pos == "right")
                    {
                        laneHighlight.Flash();
                        animator.SetTrigger("blockright");
                        return;
                    }

                    if (pos == "center") // Pressing right from center goes to right
                    {
                        pos = "right";
                        if (onMinecart)
                        {
                            minecartShiftLaneSFX.panStereo = 0.7f;
                            minecartShiftLaneSFX.Play();
                        }
                    }
                    else if (pos == "left") // Pressing right when at left goes to center
                    {
                        pos = "center";
                        if (onMinecart)
                        {
                            minecartShiftLaneSFX.panStereo = 0f;
                            minecartShiftLaneSFX.Play();
                        }
                    }
                    printCodeScript.SetCodePrompt("right");
                }
            }
        }
        else if (onForklift)
        {
            UpdateActiveCollider();
            float steerSpeed = 8f;       // sideways drift speed
            float rotationSmooth = 5f;   // how fast rotation catches up
            float tiltAngle = 20f;       // tilt amount

            float moveInput = 0f;
            if (RideForklift.isRepairing) return;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveInput = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveInput = 1f;

            // --- Position ---
            if (moveInput != 0f)
            {
                forkliftOffsetX += moveInput * steerSpeed * Time.deltaTime;
                forkliftOffsetX = Mathf.Clamp(forkliftOffsetX, leftLane, rightLane);
            }

            transform.position = new Vector3(forkliftOffsetX, transform.position.y, transform.position.z);

            // --- Rotation ---
            float desiredTilt = tiltAngle * moveInput;  // tilt only if moving
            Quaternion targetRot = Quaternion.Euler(0f, desiredTilt, 0f);

            // If no input → reset rotation to forward (0°)
            if (moveInput == 0f)
                targetRot = Quaternion.identity;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmooth * Time.deltaTime);

            // Leave forklift
            if (Input.GetKeyDown(KeyCode.UpArrow) || forkliftManager == null)
            {
                forkliftManager.ExitForklift();
                transform.rotation = startRotation;
                animator.SetTrigger("jumpoffminecart");
                UpdateActiveCollider();
                forkliftManager = null;
                Debug.Log("Left Forklift");
            }
        }

        // pos interpolator
        switch (pos)
        {
            case "left": targetpos = -3f; break;
            case "center": targetpos = 0f; break;
            case "right": targetpos = 3f; break;
        }

        // Move horizontally without touching Y
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetpos, transform.position.y, transform.position.z), horizontalSpeed * Time.deltaTime);

        // Crouching
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (!startedrunning)
                StartPlay();

            if (tutorialcard == "crouch") tutorial2d.transform.Find(tutorialcard).gameObject.SetActive(false);
            if (!isRolling)
            {
                SetCrouching(true);
                animator.SetBool("isrolling", true);
                StartCoroutine(RollSequence());
                printCodeScript.SetCodePrompt("crouch");
            }
        }

        // Jumping
        if (!isJumping && !isFlying && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            if (!startedrunning)
                StartPlay();

            if (tutorialcard == "jump") tutorial2d.transform.Find(tutorialcard).gameObject.SetActive(false);

            SetJumping(true);
            jumpStarted = Time.time;  // Track start time
            animator.SetTrigger("jump");
            if (onSkateboard && skateboardManager != null)
                skateboardManager.SkateJump();
            printCodeScript.SetCodePrompt("jumpsequence");
        }

        // Jump timing fallback (so we don't rely solely on animator transitions)
        if (isJumping)
        {
            float jumpDuration = 0.6f; // set to your clip length
            if (Time.time - jumpStarted >= jumpDuration)
            {
                printCodeScript.SetCodePrompt("jumpsequenceend");
                SetJumping(false);
            }
        }

        // Flying
        if (isFlying)
            startY = interpolateValueY(true, startY, targetHeight, 1f);

        // Holding
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
        {
            tutorial2d.transform.Find("fly").gameObject.SetActive(false);
            holding = true;
            startedrunning = true;
        }
        else holding = false;

        // Raycast ground detection
        UpdateGroundTracking();
        if (!isJumping && !isFlying)
            ApplyVerticalMovement();

        if (isUnderwater && !isDead && !endSequenceStarted)
        {
            moveSpeed = 0f;
            animator.SetBool("isrunning", false);

            if (onForklift)
            {
                animator.SetBool("isdrivingminecart", false);
                forkliftManager.ExitForklift();
                onForklift = false;
                UpdateActiveCollider();
            }

            isDead = true;
            animator.SetTrigger("endlessfall");
            printCodeScript.SetCodePrompt("dead");
            collectableControl.HandlePlayerDeath();
            StartCoroutine(EnableEndSequenceSafely());
        }

    }

    // Trigger processing forwarded from HitLogic (child)
    public void ProcessTrigger(Collider other)
    {
        HideAllTutorialCards();

        if (other.gameObject.CompareTag("obstacle") && !onForklift)
        {
            if (!godmode)
            {
                hit = true;
                string collidedObjectName = other.gameObject.name;
                printCodeScript.UpdateObstacleList(collidedObjectName);
                hurtMaskScript.Mask();
                remainingHealth--;
                Debug.Log("Entered in collision with " + other);

                if (remainingHealth <= 0)
                {
                    collectableControl.HandlePlayerDeath();
                    printCodeScript.SetCodePrompt("dead");
                    camAnimator.SetBool("dead", true);
                    isDead = true;
                    if (playerObject != null)
                        playerObject.transform.SetParent(null); // unparent from Player
                    if (onForklift)
                        StartCoroutine(RaisePlayerBody(-0.35f, 0.4f));
                    animator.SetTrigger("die");
                    crashThud.Play();
                    levelControl.GetComponent<GenerateSandstorm>().enabled = false;
                    HideAllTutorialCards();
                    StartCoroutine(EnableEndSequenceSafely());
                    RemoveHeartsInReverseOrder();
                    this.enabled = false;
                }

                else if (hit && remainingHealth > 0) // hurt
                {
                    printCodeScript.SetCodePrompt("hurt");
                    animator.SetBool("ishurt", true);
                    if (onSkateboard)
                        skateboardManager.ExitSkateboard();
                    StartCoroutine(HurtSequence());
                    HurtSFX.Play();
                    RemoveHeartsInReverseOrder();
                }
                hit = false;
            }
            else
                sphereScript.BrightUpSphere();
        }

        if (other.gameObject.CompareTag("coin"))
        {
            CollectableControl.coinCount += bonus ? 3 : 1;
            collectableControl.OnCoinCollected();
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.CompareTag("floating coin"))
        {
            coinFX.Play();

            // pitch shift of collected floating coins
            if (coinFX.pitch < 2) coinFX.pitch += 0.2f; else coinFX.pitch = 1;
            StartCoroutine(PitchShiftTimeout());
            CollectableControl.coinCount += bonus ? 3 : 1;
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.CompareTag("powerup"))
        {
            printCodeScript.SetCodePrompt("fly");
            godmode = true;
            if (onForklift)
            {
                forkliftManager.ExitForklift();
                UpdateActiveCollider();
                forkliftManager = null;
                Debug.Log("Powerup - Exited Forklift");
            }
            if (onSkateboard)
                skateboardManager.ExitSkateboard();
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 2, 0));
            flyFX.Play();
            BGM.pitch += 0.5f;
            animator.SetBool("isflying", true);
            camAnimator.SetBool("flying", true);
            if (!isFlying)
            {
                // Create array of coins
                // Calculate currentZ based on the relative position of the player to the map
                float currentZ = MAP.transform.InverseTransformPoint(this.transform.position).z + 230;
                for (int i = 0; i < flycoinsamount; i++)
                {
                    GameObject newcoin = Instantiate(flycoin, Vector3.zero, Quaternion.identity);
                    newcoin.transform.localPosition = new Vector3(this.transform.position.x, targetHeight, currentZ + (i * 3));
                    newcoin.transform.SetParent(coinContainer, false);
                    instantiatedCoins.Add(newcoin);
                }
                StartCoroutine(FlyTimeout());
            }
            isFlying = true;
            playerBody.isKinematic = true;
        }

        if (other.gameObject.CompareTag("crate"))
        {
            if (!bonus)
            {
                bonusSFX.Play();
                bonus = true;
                other.gameObject.SetActive(false);
                SetBonusLight(true);
            }
        }

        if (other.gameObject.CompareTag("pyramids") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying) pyramidsTheme.Play();

        if (other.gameObject.CompareTag("cogfactory") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying) cogFactorySFX.Play();

        if (other.gameObject.CompareTag("cogsfarm") && !mainTheme.isPlaying) cogsfarmSFX.Play();

        if (other.gameObject.CompareTag("photos") && !mainTheme.isPlaying && !photosSFX.isPlaying) photosSFX.Play();

        if (other.gameObject.CompareTag("backdoor") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying && !photosSFX.isPlaying) backDoorSFX.Play();

        if (other.gameObject.CompareTag("cardboard")) (Random.value < 0.5f ? cardboard1 : cardboard2).Play();

        if (other.gameObject.CompareTag("panoptic"))
        {
            if (!alreadyCrossedPanoptic)
            {
                StartCoroutine(ApplyGlissando());
                alreadyCrossedPanoptic = true;
            }
            else
                if (Random.value >= 0.5f) StartCoroutine(ApplyGlissando());

            if (!mainTheme.isPlaying && !panopticSFX.isPlaying && !canyonSFX.isPlaying && !pyramidsTheme.isPlaying) panopticSFX.Play();
        }

        if (other.gameObject.CompareTag("canyon") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying && !canyonSFX.isPlaying) canyonSFX.Play();
        
        if (other.gameObject.CompareTag("claxon")) claxonSFX.Play();

        if (other.gameObject.CompareTag("car") && !godmode)
        {
                camAnimator.SetBool("dead", true);
                if (onForklift)
                    StartCoroutine(RaisePlayerBody(-0.35f, 0.4f));
                animator.SetTrigger("die");
            string collidedObjectName = other.gameObject.name;
            printCodeScript.UpdateObstacleList(collidedObjectName);
            printCodeScript.SetCodePrompt("dead");
            isDead = true;
                if (playerObject != null)
                    playerObject.transform.SetParent(null); // unparent from Player
                animator.SetBool("isrunning", false);
                carCrashSFX.Play();
                HideAllTutorialCards();
                hitLogic.EnableHitbox(HitLogic.HitboxType.None);
                collectableControl.HandlePlayerDeath();
                StartCoroutine(EnableEndSequenceSafely());
                this.enabled = false; // Disable this script
        }

        if (other.gameObject.CompareTag("minewall") && !triggered)
        {
            triggered = true;
            camAnimator.SetBool("dead", true);
            if (onForklift)
                StartCoroutine(RaisePlayerBody(-0.35f, 0.4f));
            animator.SetTrigger("die");
            string collidedObjectName = other.gameObject.name;
            printCodeScript.UpdateObstacleList(collidedObjectName);
            printCodeScript.SetCodePrompt("dead");
            isDead = true;
            if (playerObject != null)
                playerObject.transform.SetParent(null); // unparent from Player
            if (onMinecart)
            {
                moveSpeed = 0f;
                minecartCrashSFX.Play();
                carCrashSFX.Play();
                minecart.CartCrash();
                minecartObject.Stop();
                onMinecart = false;
            }
            else
            {
                animator.SetBool("isrunning", false);
                carCrashSFX.Play();
            }
            rocks.SetActive(true);
            Debug.Log("Entered in minewall collision with " + other);
            HideAllTutorialCards();
            collectableControl.HandlePlayerDeath();
            StartCoroutine(EnableEndSequenceSafely());
            this.enabled = false;
        }

        if (other.gameObject.CompareTag("tutorial"))
        {
            HideAllTutorialCards();
            tutorialcard = other.gameObject.name;
            Transform tutorialCardTransform = tutorial2d.transform.Find(tutorialcard);
            if (tutorialCardTransform != null)
            {
                tutorialCardTransform.gameObject.SetActive(true);
                if (tutorialInstructions.TryGetValue(tutorialcard, out string instruction))
                    DisplayInstruction(instruction);
                else
                    Debug.LogError("Instruction not found for tutorial card: " + tutorialcard);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!onForklift) return;

        if (other.CompareTag("obstacle"))
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
                animator.SetBool("ishurt", true);

            hurtMaskScript.Mask();
            forkliftManager.ApplyCollisionDamage(Time.deltaTime);
            soundTimer -= Time.deltaTime;   // no està clar
            if (soundTimer <= 0f && !isDead)
            {
                PlayRandomHitSound();
                soundTimer = soundInterval;
            }
        } else if (other.CompareTag("fall"))
            DieOnForklift();
    }
    // forklift's pushing force
    private void OnCollisionStay(Collision collision)
    {
        if (!onForklift) return;
        Rigidbody objrb = collision.rigidbody;
        if (objrb != null && objrb.mass <= 100f && !objrb.isKinematic)
        {
            Vector3 pushDir = transform.forward;
            objrb.AddForce(pushDir * 15f, ForceMode.Impulse);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!onForklift) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            animator.SetBool("ishurt", false);
    }

    private void PlayRandomHitSound()
    {
        if (hitClips.Length == 0 || rumbleSFX == null) return;
        int index = Random.Range(0, hitClips.Length);
        rumbleSFX.pitch = Random.Range(0.8f, 1.1f);
        rumbleSFX.PlayOneShot(hitClips[index]);
    }

    public void DieOnForklift()
    {
        Debug.Log("Player Dead on Forklift");
        printCodeScript.SetCodePrompt("dieonforklift");
        StartCoroutine(RaisePlayerBody(-0.35f, 0.4f));
        isDead = true;
        hitLogic.EnableHitbox(HitLogic.HitboxType.None);
        if (playerObject != null)
            playerObject.transform.SetParent(null); // unparent from Player
        animator.SetTrigger("die");
        StartCoroutine(DelayedHandlePlayerDeath(1.5f));
    }

    private IEnumerator DelayedHandlePlayerDeath(float delay)
    {
        yield return new WaitForSeconds(delay);
        collectableControl.HandlePlayerDeath();
        StartCoroutine(EnableEndSequenceSafely());
        onForklift = false;
        this.enabled = false;
    }

    public void StartPlay()
    {
            idle = false;
            startedrunning = true;
            BGM.Play();
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", 3, 0.7f));
            if (!mainThemeAlreadyPlaying) StartCoroutine(PlayMainTheme());
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 1.5f, 1));
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 1.5f, 1));
            tutorial2d.transform.Find("touch-cards").gameObject.SetActive(false);
            startingText.SetActive(false);
            printCodeScript.SetCodePrompt("start");
    }

    private void DisplayInstruction(string instruction)
    {
        tutorialText.GetComponent<Text>().text = instruction;
        tutorialText.SetActive(true);
    }

    public void SetJumping (bool jumping)
    {
        isJumping = jumping;
        if (hitLogic != null)
        {
            if (jumping)
            {
                playerBody.isKinematic = true;
                gameObject.layer = LayerMask.NameToLayer("PlayerJumping");
                hitLogic.EnableHitbox(HitLogic.HitboxType.Jump);
            }
            else
            {
                playerBody.isKinematic = false;
                gameObject.layer = LayerMask.NameToLayer("Player");
                hitLogic.EnableHitbox(HitLogic.HitboxType.Normal);
            }
        }
    }

    public void SetCrouching(bool crouching)
    {
        isRolling = crouching;
        if (hitLogic != null)
        {
            if (crouching)
                hitLogic.EnableHitbox(HitLogic.HitboxType.Crouch);
            else
                hitLogic.EnableHitbox(HitLogic.HitboxType.Normal);
        }
    }

    IEnumerator RollSequence()
    {
        if (onSkateboard)
            yield return new WaitForSeconds(1.35f);
        else
            yield return new WaitForSeconds(0.9f);
        SetCrouching(false);
        animator.SetBool("isrolling", false);
    }

    IEnumerator HurtSequence()
    {
        hitLogic.EnableHitbox(HitLogic.HitboxType.None);
        yield return new WaitForSeconds(0.3f);
        hitLogic.EnableHitbox(HitLogic.HitboxType.Normal);
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("ishurt", false);
    }

    IEnumerator FlyTimeout()
    {
        yield return new WaitForSeconds(3);
        tutorial2d.transform.Find("fly").gameObject.SetActive(true);

        yield return new WaitForSeconds(5);
        tutorial2d.transform.Find("fly").gameObject.SetActive(false);

        while (holding) yield return new WaitForSeconds(1);

        camAnimator.SetBool("flying", false);
        StartCoroutine(delayedGodmodeOff(5f, 3f));
        StartCoroutine(ChangePitchOverTime());
        animator.SetBool("isflying", false);

        // End flying immediately and let physics take over
        isFlying = false;
        playerBody.isKinematic = false;

        // give a small downward velocity to ensure gravity immediately affects the player
        Vector3 v = playerBody.velocity;
        playerBody.velocity = new Vector3(v.x, -2f, v.z);

        moveSpeed = initialmoveSpeed;

        foreach (GameObject coin in instantiatedCoins) coin.SetActive(false);
        instantiatedCoins.Clear();
    }

    IEnumerator PitchShiftTimeout()
    {
        yield return new WaitForSeconds(1);
        coinFX.pitch = 1;
    }

    IEnumerator ChangePitchOverTime()
    {
        float startTime = Time.time;

        while (Time.time - startTime < pitchDuration)
        {
            float t = (Time.time - startTime) / pitchDuration;
            BGM.pitch = Mathf.Lerp(startingPitch, endingPitch, t);
            yield return null;
        }

        BGM.pitch = endingPitch;
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 3, 1f));
    }

    IEnumerator ApplyGlissando()
    {
        camAnimator.SetBool("panoptic", true);
        printCodeScript.SetCodePrompt("panoptic");

        float halfDuration = 4.0f;
        float elapsedTime = 0f;

        // Gradually increase the pitch from pitchMin to pitchMax
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;
            BGM.pitch = Mathf.Lerp(1f, 5f, t);
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", t, 1f));
            yield return null;
        }
        elapsedTime = 0f;

        // Gradually decrease the pitch from pitchMax back to pitchMin
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;
            BGM.pitch = Mathf.Lerp(5.5f, 1f, t);
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeBGM", t, 0.7f));
            yield return null;
        }
        BGM.pitch = endingPitch;
        yield return new WaitForSeconds(6);
        camAnimator.SetBool("panoptic", false);
    }

    IEnumerator PlayMainTheme()
    {
        yield return new WaitForSeconds(18);

        if (isOnModernTimes)
            yield break;

        if (!idle && !mainTheme.isPlaying && !pyramidsTheme.isPlaying && !flyFX.isPlaying)
        {
            mainTheme.Play();
            mainThemeAlreadyPlaying = true;

            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 2, 0f));

            float elapsed = 0f;
            const float totalDuration = 50f;

            while (elapsed < totalDuration)
            {
                if (isOnModernTimes)
                {
                    // Fade out and stop immediately if player enters Modern Times
                    StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 1.5f, 0f));
                    mainTheme.Stop();
                    mainThemeAlreadyPlaying = false;
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 3, 1f));
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeThemes", 3, 1f));
        }
    }

    IEnumerator delayedGodmodeOff(float godmodeDuration, float blinkDuration)
    {
        godmode = true;
        godmodevisual.SetActive(true);
        godmodevisual.GetComponent<ToggleShield>().shield.enabled = true;
        yield return new WaitForSeconds(godmodeDuration);
        godmodevisual.GetComponent<ToggleShield>().enabled = true;
        yield return new WaitForSeconds(blinkDuration);
        godmode = false;
        godmodevisual.SetActive(false);
        godmodevisual.GetComponent<ToggleShield>().enabled = false;
        godmodevisual.GetComponent<ToggleShield>().shield.enabled = false;
        if (bonus)
        {
            bonus = false;
            SetBonusLight(false);
        }
    }

    public IEnumerator RaisePlayerBody(float targetY, float duration)
    {
        Transform body = transform.Find("Ch46_nonPBR@Standard Run");
        Vector3 startPos = body.localPosition;
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            body.localPosition = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }
        body.localPosition = endPos;
    }

    private IEnumerator EnableEndSequenceSafely()
    {
        yield return null; // wait one frame
        levelControl.GetComponent<EndRunSequence>().enabled = true;
        endSequenceStarted = true;
    }

    private float interpolateValueY(bool easingOut = true, float origin = 0.0f, float target = 5.0f, float intspeed = 0.2f)
    {
        float fraction = Time.deltaTime * intspeed;

        if (easingOut)
        {
            fraction = 1 - Mathf.Pow(1 - fraction, 3);
            if (moveSpeed < 30 - 0f) moveSpeed += fraction * 10;
        }
        else
        {
            fraction = Mathf.Pow(fraction, 0.9f); // Adjust the power to make the easing smoother (the smaller the faster)
            if (moveSpeed > 12.0f) moveSpeed -= fraction * 2;
        }

        float currentY = Mathf.Lerp(origin, target, fraction);
        transform.position = new Vector3(transform.position.x, currentY, transform.position.z);
        origin = currentY;
        return origin;
    }

    private void HideAllTutorialCards()
    {
        tutorialText.SetActive(false);
        foreach (Transform child in tutorial2d.transform) child.gameObject.SetActive(false);
    }

    private void UpdateActiveCollider()
    {
        if (onForklift)
        {
            if (forkliftCollider != null) forkliftCollider.enabled = true;
            if (boxCollider != null) boxCollider.enabled = false;
        }
        else
        {
            if (forkliftCollider != null) forkliftCollider.enabled = false;
            if (boxCollider != null) boxCollider.enabled = true;
        }
        if (isUnderwater)
        {
            if (forkliftCollider != null) forkliftCollider.enabled = false;
            if (boxCollider != null) boxCollider.enabled = false;
        }
    }

    public void ClearSkateboard()
    {
        if (skateboardManager != null)
            skateboardManager.ClearSkateboard();
    }


    // RAYCAST

    void UpdateGroundTracking()
    {
        if (boxCollider == null) return;
        float feetOffset = transform.position.y + boxCollider.center.y - (boxCollider.size.y / 2f);
        Vector3 rayOrigin = new Vector3(transform.position.x, feetOffset + raycastHeightOffset, transform.position.z);

        // Ground detection
        Ray ray = new Ray(rayOrigin, Vector3.down);
        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, groundLayer)) isGrounded = true;
        else isGrounded = false;

        // Wall detection
        Ray leftRay = new Ray(rayOrigin, Vector3.left);
        Ray rightRay = new Ray(rayOrigin, Vector3.right);
        Debug.DrawRay(rayOrigin, Vector3.left * rayLength*2, Color.green);
        Debug.DrawRay(rayOrigin, Vector3.right * rayLength*2, Color.green);

        if (Physics.Raycast(leftRay, out RaycastHit leftHit, rayLength * 2, wallLayer))
        {
            blockLeft = true;
            //Debug.Log("Left Ray hit: " + leftHit.collider.name);
        }
        else blockLeft = false;

        if (Physics.Raycast(rightRay, out RaycastHit rightHit, rayLength * 2, wallLayer))
        {
            blockRight = true;
            //Debug.Log("Right Ray hit: " + rightHit.collider.name);
        }
        else blockRight = false;
    }

    void ApplyVerticalMovement()
    {
        if (!isGrounded)
        {
            isFalling = true;
            animator.SetBool("isfalling", true);
            playerBody.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
        else
        {
            if (isFalling)
            {
                animator.SetBool("isfalling", false);   //back to running
                isFalling = false;
            }
        }
        // fallback in case player falls beyond trigger
        if (isFalling)
        {
            fallTimer += Time.deltaTime;
            if (fallTimer >= 10f && !isDead && !endSequenceStarted)
            {
                moveSpeed = 0f;
                isDead = true;
                animator.SetTrigger("endlessfall");
                collectableControl.HandlePlayerDeath();
                StartCoroutine(EnableEndSequenceSafely());
                printCodeScript.SetCodePrompt("longfall");
                fallTimer = 0f;
            }
        }
        else
            fallTimer = 0;
    }

    private void StopThemes()
    {
        StopCoroutine(PlayMainTheme());
        if (BGM.isPlaying)
            BGM.Stop();
        if (mainTheme.isPlaying)
            mainTheme.Stop();
        if (pyramidsTheme.isPlaying)
            pyramidsTheme.Stop();
        if (panopticSFX.isPlaying)
            panopticSFX.Stop();
    }

    public void SetBonusLight(bool enabled)
    {
        bonusSpotlight.enabled = enabled;

        var renderer = godmodevisual.GetComponent<MeshRenderer>();
        renderer.material = enabled ? bonusMaterial : godModeMaterial;
        if (enabled)
            StartCoroutine(delayedGodmodeOff(8f, 3f));

        float startIntensity = enabled ? 1.5f : 1f;
        float endIntensity = enabled ? 1f : 1.5f;

        StartCoroutine(DimLight(startIntensity, endIntensity));
        bonusUI.SetActive(enabled);
    }

    private IEnumerator DimLight(float startingIntensity, float endingIntensity)
    {
        float startDimTime = Time.time;

        while (Time.time - startDimTime < 3f)
        {
            float t = Time.time - startDimTime;
            globalLight.intensity = Mathf.Lerp(startingIntensity, endingIntensity, t);
            yield return null;
        }
        globalLight.intensity = endingIntensity;
    }

    public void ResetState()
    {
        //Debug.Log("PlayerMove reset");
        levelControl.GetComponent<EndRunSequence>().enabled = false;

        // set bools
        onForklift = false;
        onSkateboard = false;
        isUnderwater = false;
        isOnModernTimes = false;
        isInTheSandstorm = false;
        boxCollider.enabled = true;
        forkliftCollider.enabled = false;
        godmodevisual.SetActive(false);
        godmodevisual.GetComponent<MeshRenderer>().material = godModeMaterial;
        godmode = false;
        startedrunning = false;
        idle = true;
        isDead = false;
        endSequenceStarted = false;
        onMinecart = false;
        triggered = false;
        globalLight.intensity = 1.5f;
        bonus = false;
        bonusSpotlight.enabled = false;
        bonusUI.SetActive(false);
        rayLength = 1.2f;
        endstormText.SetActive(false);

        // destroy vehicles
        Transform forkliftHolder = transform.Find("forklift");
        if (forkliftHolder != null)
        {
            for (int i = forkliftHolder.childCount -1; i>= 0; i--)
                forkliftHolder.GetChild(i).gameObject.SetActive(false);
        }
        Transform minecartHolder = transform.Find("minecart");
        if (minecartHolder != null)
        {
            for (int i = minecartHolder.childCount - 1; i >= 0; i--)
                minecartHolder.GetChild(i).gameObject.SetActive(false);
        }

        // set tutorial timers/ text
        timer = 0f;
        tutorialcard = "";
        HideAllTutorialCards();

        // set animator states
        animator.Rebind();
        camAnimator.Rebind();

        // set position
        if (rocks.activeSelf)
            rocks.SetActive(false);
        transform.position = startPosition;
        startY = transform.position.y;
        transform.rotation = startRotation;
        moveSpeed = 12f;
        StartCoroutine(RaisePlayerBody(-0.35f, 0f));
        if (playerObject != null && playerStartParent != null)
        {
            playerObject.transform.SetParent(playerStartParent); // reparent
            playerObject.transform.localPosition = playerLocalPos;
            playerObject.transform.localRotation = playerLocalRot;
        }

        // set hitboxes
        if (hitLogic != null)
            hitLogic.EnableHitbox(HitLogic.HitboxType.Normal);
        playerBody.isKinematic = false;

        // set audio parameters
        BGM.pitch = 1.0f;
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSandstorm", 0, 0));
        StopThemes();
        
        mainThemeAlreadyPlaying = false;
        alreadyCrossedPanoptic = false;

        // set log spacing
        Debug.Log("\n\n\n");
        Debug.Log("========================================");
        Debug.Log($"GAME STARTED: {System.DateTime.Now}");
        Debug.Log("========================================");

        // set health
        foreach (var h in heartList) Destroy(h);
        heartList.Clear();
        remainingHealth = 0;
        for (int i = 0; i < maxHealth; i++)
            AddHeart();
    }
}