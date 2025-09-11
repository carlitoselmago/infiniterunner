using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PlayerMove : MonoBehaviour, IResettable
{
    private Vector3 startPosition = new Vector3(0f, -0.35f, -48f);
    public float moveSpeed = 12.0f;
    private float initialmoveSpeed = 0;
    public float horizontalSpeed = 20f;
    public bool isJumping = false;
    public bool isRolling = false;
    public bool isFlying = false;
    //public bool floating = false;
    public bool holding = false;
    public static bool onMinecart = false;
    private bool mainThemeAlreadyPlaying = false;
    public static bool idle = true;
    public static bool isUnderwater = false;

    [Header("Constrains")]  //(from Constrain.cs)
    public bool blockLeft = false;
    public bool blockCenter = false;
    public bool blockRight = false;

    // raycast
    [Header("Raycast")]
    public LayerMask groundLayer;
    public float rayLength = 0.7f;
    public float raycastHeightOffset = 0.5f;
    public bool isGrounded = false;
    public bool isFalling = false;

    [Header("Health")]
    public static int maxHealth = 5;
    public static int remainingHealth;
    private bool hit = false;
    public static bool isDead = false;
    public bool godmode = false;

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

    public HitLogic hitLogic; // assign the child HitLogic in inspector or it will auto-find

    private float targetHeight = 17.0f;
    private float startY;
    private float originY;
    private float jumpedHeight;

    private float jumpStarted;

    public GameObject tutorial2d;
    private float timer;
    private bool alreadyCrossedPanoptic = false;
    public AudioSource coinFX;
    public HurtMask hurtMaskScript;
    public AudioSource minecartObject;
    private bool triggered = false;

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
        //startPosition = transform.position;
        animator = GetComponentInChildren<Animator>();
        camAnimator = mainCam.GetComponent<Animator>();

        // Ensure playerBody assigned
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
        originY = startY;
        onMinecart = false;
        BGM.pitch = 1.0f;
        HideAllTutorialCards();
        isDead = false;
        remainingHealth = 0;
        startedrunning = false;
        godmodevisual.SetActive(false);
        initialmoveSpeed = moveSpeed;
        collectableControl = FindObjectOfType<CollectableControl>();

        //set hearts based on amount of life
        for (int i = 0; i < maxHealth; i++)
            AddHeart();
    }

    public void AddHeart()
    {
        if (heartList.Count < maxHealth)
        {
            //Debug.Log("added heart!!!!");
            GameObject clonedHeart = Instantiate(heart, Vector3.zero, Quaternion.identity);
            clonedHeart.transform.SetParent(hearts.transform, false);
            clonedHeart.transform.localPosition = new Vector3(heartList.Count * 50, 0, 0);
            Animator heartAnimator = clonedHeart.GetComponent<Animator>();
            heartList.Add(clonedHeart);
            heartAnimator.SetBool("started", true);
            remainingHealth += 1;
        }
        else
            Debug.Log("Cannot add more hearts");
    }

    public void RemoveHeartsInReverseOrder()
    {
        int lastindex = heartList.Count - 1;
        Destroy(heartList[lastindex]);
        heartList.RemoveAt(lastindex);
    }

    void Update()

    {
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
                }
            }
        }

        if (startedrunning && !animator.GetBool("isrunning"))
            animator.SetBool("isrunning", true);

        if (animator.GetBool("isrunning"))
            MAP.transform.Translate(Vector3.back * Time.deltaTime * moveSpeed, Space.World);

        if (playerBody.IsSleeping())
            playerBody.WakeUp();

        // Left
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!startedrunning)
                StartPlay();

            if (tutorialcard == "left") tutorial2d.transform.Find(tutorialcard).gameObject.SetActive(false);
            if (!isFlying)
            {
                if (pos == "center") // Pressing left from center goes to left
                {
                    if (blockLeft) return;
                    pos = "left";
                    if (onMinecart)
                    {
                        minecartShiftLaneSFX.panStereo = -0.7f;
                        minecartShiftLaneSFX.Play();
                    }
                }
                else if (pos == "right") // Pressing left when at right goes to center
                {
                    if (blockCenter) return;
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
                if (pos == "center") // Pressing right from center goes to right
                {
                    if (blockRight) return;
                    pos = "right";
                    if (onMinecart)
                    {
                        minecartShiftLaneSFX.panStereo = 0.7f;
                        minecartShiftLaneSFX.Play();
                    }
                }
                else if (pos == "left") // Pressing right when at left goes to center
                {
                    if (blockCenter) return;
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
            printCodeScript.SetCodePrompt("jumpsequence");
            }

        // Jump timing fallback (so we don't rely solely on animator transitions)
        if (isJumping)
        {
            float jumpDuration = 0.6f; // set to your clip length
            if (Time.time - jumpStarted >= jumpDuration)
                SetJumping(false);
        }

        // Flying
        //if (floating) // removable bool altogether
        //    jumpedHeight = interpolateValueY(false, jumpedHeight, originY, 2.8f);
        //else
        //{
            // only interpolate while rising
            if (isFlying)
                startY = interpolateValueY(true, startY, targetHeight, 1f);
        //}

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
        if (!isJumping && !isFlying /*&& !floating*/)
            ApplyVerticalMovement();
    }

    // Trigger processing forwarded from HitLogic (child)
    public void ProcessTrigger(Collider other)
    {
        HideAllTutorialCards();

        if (other.gameObject.CompareTag("obstacle"))
        {
            if (!godmode)
            {
                hit = true;
                printCodeScript.SetCodePrompt("dead");
                StartCoroutine(hurtMaskScript.Mask());
                remainingHealth--;
                Debug.Log("Entered in collision with " + other);
                var bc = other.GetComponent<BoxCollider>();
                if (bc != null) bc.enabled = false;

                if (remainingHealth <= 0)
                {
                    collectableControl.HandlePlayerDeath();
                    camAnimator.SetBool("dead", true);
                    isDead = true;
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
                    StartCoroutine(HurtSequence());
                    HurtSFX.Play();
                    RemoveHeartsInReverseOrder();
                }
                hit = false;
            }
        }

        if (other.gameObject.CompareTag("coin"))
        {
            coinFX.pitch = 1;
            coinFX.Play();
            CollectableControl.coinCount += 1;
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.CompareTag("floating coin"))
        {
            coinFX.Play();

            // pitch shift of collected floating coins
            if (coinFX.pitch < 2) coinFX.pitch += 0.2f; else coinFX.pitch = 1;
            StartCoroutine(PitchShiftTimeout());
            CollectableControl.coinCount += 1;
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.CompareTag("powerup") || (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
        {
            printCodeScript.SetCodePrompt("fly");
            godmode = true;
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

        if (other.gameObject.CompareTag("pyramids") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying) pyramidsTheme.Play();

        if (other.gameObject.CompareTag("cogfactory") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying) cogFactorySFX.Play();

        if (other.gameObject.CompareTag("cogsfarm") && !mainTheme.isPlaying) cogsfarmSFX.Play();

        if (other.gameObject.CompareTag("photos") && !mainTheme.isPlaying && !photosSFX.isPlaying) photosSFX.Play();

        if (other.gameObject.CompareTag("backdoor") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying) backDoorSFX.Play();

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
                //var cb = other.GetComponent<Collider>();
                //if (cb != null) cb.enabled = false;
                camAnimator.SetBool("dead", true);
                animator.SetTrigger("die");
                carCrashSFX.Play();
                HideAllTutorialCards();
                collectableControl.HandlePlayerDeath();
                StartCoroutine(EnableEndSequenceSafely());
                this.enabled = false; // Disable this script
        }

        if (other.gameObject.CompareTag("minewall") && !triggered)
        {
            //var cb = other.GetComponent<Collider>();
            //if (cb != null) cb.enabled = false;
            camAnimator.SetBool("dead", true);
            if (onMinecart)
            {
                animator.SetTrigger("minecartcollision");
                minecartCrashSFX.Play();
                carCrashSFX.Play();
                minecartObject.Stop();
                onMinecart = false;
                triggered = true;
            }
            else
            {
                animator.SetTrigger("die");
                carCrashSFX.Play();
                triggered = true;
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

    public void SetConstrainedPositions(bool left, bool center, bool right)
    {
        //blockLeft = left;
        //blockCenter = center;
        //blockRight = right;
    }

    public void SetJumping (bool jumping)
    {
        isJumping = jumping;
        if (hitLogic != null)
        {
            if (jumping)
            {
                gameObject.layer = LayerMask.NameToLayer("PlayerJumping");
                hitLogic.EnableHitbox(HitLogic.HitboxType.Jump);
            }
            else
            {
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
        printCodeScript.SetCodePrompt("rollsequence");
        yield return new WaitForSeconds(0.45f);
        yield return new WaitForSeconds(0.45f);
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
        StartCoroutine(delayedGodmodeOff());
        StartCoroutine(ChangePitchOverTime());
        animator.SetBool("isflying", false);

        // End flying immediately and let physics take over
        isFlying = false;

        // Re-enable physics (previously set to kinematic during flying)
        playerBody.isKinematic = false;

        // give a small downward velocity to ensure gravity immediately affects the player
        // preserve horizontal components, override only y
        Vector3 v = playerBody.velocity;
        playerBody.velocity = new Vector3(v.x, -2f, v.z);

        // optional small delay removed here — physics will do the rest
        moveSpeed = initialmoveSpeed;

        foreach (GameObject coin in instantiatedCoins) coin.SetActive(false);
        instantiatedCoins.Clear();
    }

    /*
    IEnumerator FlyTimeout()
    {
        yield return new WaitForSeconds(3);
        tutorial2d.transform.Find("fly").gameObject.SetActive(true);

        yield return new WaitForSeconds(5);
        tutorial2d.transform.Find("fly").gameObject.SetActive(false);

        while (holding) yield return new WaitForSeconds(1);

        camAnimator.SetBool("flying", false);
        //mainCam.GetComponent<Animator>().SetBool("flying", false);
        StartCoroutine(delayedGodmodeOff());
        StartCoroutine(ChangePitchOverTime());
        floating = true;
        animator.SetBool("isflying", false);
        jumpedHeight = this.transform.position.y;
        yield return new WaitForSeconds(1);

        isFlying = false;
        yield return new WaitForSeconds(1);
        floating = false;
        moveSpeed = initialmoveSpeed;
        playerBody.isKinematic = false;

        foreach (GameObject coin in instantiatedCoins) coin.SetActive(false);
        instantiatedCoins.Clear();
    }*/

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
        //mainCam.GetComponent<Animator>().SetBool("panoptic", true);
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
        //mainCam.GetComponent<Animator>().SetBool("panoptic", false);
    }

    IEnumerator PlayMainTheme()
    {
        yield return new WaitForSeconds(18);
        if (!idle && !mainTheme.isPlaying && !pyramidsTheme.isPlaying && !flyFX.isPlaying)
        {
            mainTheme.Play();
            mainThemeAlreadyPlaying = true;
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 2, 0f));
            yield return new WaitForSeconds(50);
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "volumeSFX", 3, 1f));
        }
    }

    IEnumerator delayedGodmodeOff()
    {
        godmodevisual.SetActive(true);
        godmodevisual.GetComponent<ToggleShield>().shield.enabled = true;
        yield return new WaitForSeconds(5);
        godmodevisual.GetComponent<ToggleShield>().enabled = true;
        yield return new WaitForSeconds(3);
        godmode = false;
        godmodevisual.SetActive(false);
        godmodevisual.GetComponent<ToggleShield>().enabled = false;
        godmodevisual.GetComponent<ToggleShield>().shield.enabled = false;
    }

    private IEnumerator EnableEndSequenceSafely()
    {
        yield return null; // wait one frame
        levelControl.GetComponent<EndRunSequence>().enabled = true;
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

    // RAYCAST
    
    void UpdateGroundTracking()
    {
        if (boxCollider == null) return;
        float feetOffset = transform.position.y + boxCollider.center.y - (boxCollider.size.y / 2f);
        Vector3 rayOrigin = new Vector3(transform.position.x, feetOffset + raycastHeightOffset, transform.position.z);
        Ray ray = new Ray(rayOrigin, Vector3.down);
        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red);

        // Optional test: remove Ray definition and replace if statement by this:
        //if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayLength, groundLayer))

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, groundLayer)) isGrounded = true;
        else isGrounded = false;
    }

    // review so falling stops or eases out playerMove (falling without the map scrolling)
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

    public void ResetState()
    {
        Debug.Log("PlayerMove reset");
        levelControl.GetComponent<EndRunSequence>().enabled = false;

        // set bools
        boxCollider.enabled = true;
        godmodevisual.SetActive(false);
        godmode = false;
        startedrunning = false;
        idle = true;
        isDead = false;
        onMinecart = false;

        // set tutorial timers/ text
        timer = 0f;
        tutorialcard = "";
        HideAllTutorialCards();

        // set animator states
        animator.Rebind();
        camAnimator.Rebind();

        // set position
        rocks.SetActive(false);
        transform.position = startPosition;
        startY = transform.position.y;
        originY = startY;
        moveSpeed = 12.0f;
        //SetConstrainedPositions(false, false, false);

        // set hitboxes
        if (hitLogic != null)
            hitLogic.EnableHitbox(HitLogic.HitboxType.Normal);
        playerBody.isKinematic = false;

        // set audio parameters
        BGM.pitch = 1.0f;
        StopThemes();
        
        mainThemeAlreadyPlaying = false;
        alreadyCrossedPanoptic = false;

        // set health
        foreach (var h in heartList) Destroy(h);
        heartList.Clear();
        remainingHealth = 0;
        for (int i = 0; i < maxHealth; i++)
            AddHeart();
    }
}