using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 12.0f;
    private float initialmoveSpeed = 0;
    public float leftRightSpeed = 10.0f;
    public bool isJumping = false;
    public bool comingDown = false;
    public bool isRolling = false;
    public bool standingUp = false;
    public static bool canFly = false;
    public bool nowCanFly = false;
    public bool isFlying = false;
    public bool floating = false;
    public bool holding = false;
    private bool mainThemeAlreadyPlaying = false;
    // move constrains (from Constrain.cs)
    private bool blockLeft = false;
    private bool blockCenter = false;
    private bool blockRight = false;

    // raycast
    public LayerMask groundLayer;
    private float fallSpeed = 20.0f;
    private float rayLength = 1.0f;
    private float raycastHeightOffset = 0.5f;
    private float verticalVelocity = 0f;
    public bool isGrounded = false;
    public bool isFalling = false;

    public static int maxHealth = 3;
    public static int remainingHealth;
    private bool hit = false;
    public static bool isDead = false;
    public bool godmode = false;
    public int flycoinsamount = 30;

    private List<GameObject> instantiatedCoins = new List<GameObject>();

    public GameObject godmodevisual;
    public GameObject playerObject;
    public Rigidbody playerBody;
    public GameObject startingText;
    public GameObject tutorialText;
    private Animator animator;

    public GameObject mainCam;

    //sfx
    public AudioSource HurtSFX;
    public AudioSource crashThud;
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

    //pitch shifter for flying timeout
    private float startingPitch = 1.5f;
    private float endingPitch = 1f;
    private float pitchDuration = 0.8f;

    //audio mixer
    public AudioMixer audioMixer;
    private string exposedParameter;
    private float duration;
    private float targetVolume;

    public GameObject levelControl;
    public GameObject triggeredObject;
    private BoxCollider boxCollider;

    private float targetHeight = 17.0f;
    private float startY;
    private float originY;
    private float jumpedHeight;

    public GameObject flycoin;

    public GameObject tutorial2d;
    private float timer;
    private bool alreadyCrossedPanoptic = false;
    public AudioSource coinFX;
    public GameObject objectWithMoveScript;
    public HurtMask hurtMaskScript;

    public GameObject MAP;

    public GameObject hearts;
    public GameObject heart;

    // List to store instantiated hearts
    public List<GameObject> heartList = new List<GameObject>();

    public float horizontalSpeed = 20f;

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
        boxCollider = GetComponent<BoxCollider>();
        startY = transform.position.y;
        originY = startY;
        normalhitbox();
        BGM.pitch = 1.0f;
        HideAllTutorialCards();
        isDead = false;
        remainingHealth = 0;
        startedrunning = false;
        godmodevisual.SetActive(false);
        initialmoveSpeed = moveSpeed;

        //set hearts based on amount of life
        for (int i = 0; i < maxHealth; i++)
        {
            AddHeart();
        }
    }

    public void AddHeart()
    {
        if (heartList.Count < maxHealth)
        {
            Debug.Log("added heart!!!!");
            // Instantiate the heart prefab at the specified location
            GameObject clonedHeart = Instantiate(heart, Vector3.zero, Quaternion.identity);

            // Set the parent of the cloned heart
            clonedHeart.transform.SetParent(hearts.transform, false);

            // Optionally adjust the position if you want to stagger them or place them differently
            clonedHeart.transform.localPosition = new Vector3(heartList.Count * 50, 0, 0);

            // Get the Animator component of the cloned heart
            Animator heartAnimator = clonedHeart.GetComponent<Animator>();
            heartList.Add(clonedHeart);
            heartAnimator.SetBool("started", true);
            remainingHealth += 1;
        }
        else
        {
            Debug.Log("Cannot add more hearts");
        }
    }

    public void RemoveHeartsInReverseOrder()
    {
        int lastindex = heartList.Count - 1;
        // Destroy the heart GameObject
        Destroy(heartList[lastindex]);

        // Remove the heart from the list
        heartList.RemoveAt(lastindex);
    }

    void Update()

    {
        if (startedrunning == false)
        {
            if (startingText.active == false)
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

        if (startedrunning == false && Input.anyKey == true)
        {
            BGM.Play();
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 3, targetVolume = 0.7f));
            if (mainThemeAlreadyPlaying == false)
            {
                StartCoroutine(PlayMainTheme());
            }
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 1.5f, targetVolume = 1));
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeSFX", duration = 1.5f, targetVolume = 1));
            tutorial2d.transform.Find("touch-cards").gameObject.SetActive(false);
            startingText.SetActive(false);
            printCodeScript.SetCodePrompt("start");
        }
        if (startedrunning == true && !animator.GetBool("isrunning"))
        {
            animator.SetBool("isrunning", true);
        }
        if (animator.GetBool("isrunning"))
        {
            MAP.transform.Translate(Vector3.back * Time.deltaTime * moveSpeed, Space.World);
        }

        if (playerBody.IsSleeping())
        {
            playerBody.WakeUp();
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            startedrunning = true;
            if (tutorialcard == "left")
            {
                tutorial2d.transform.Find(tutorialcard).gameObject.SetActive(false);
            }
            if (!isFlying)
            {
                if (pos == "center" && transform.position.x == 0f) // Pressing left from center goes to left
                {
                    if (blockLeft) return;
                    pos = "left";
                }
                else if (pos == "right") // Pressing left when at right goes to center
                {
                    if (blockCenter) return;
                    pos = "center";
                }
                printCodeScript.SetCodePrompt("left");
            }
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            startedrunning = true;
            if (tutorialcard == "right")
            {
                tutorial2d.transform.Find(tutorialcard).gameObject.SetActive(false);
            }
            if (!isFlying)
            {
                if (pos == "center" && transform.position.x == 0f) // Pressing right from center goes to right
                {
                    if (blockRight) return;
                    pos = "right";
                }
                else if (pos == "left") // Pressing right when at left goes to center
                {
                    if (blockCenter) return;
                    pos = "center";
                }
                printCodeScript.SetCodePrompt("right");
            }
        }

        // pos interpolator
        switch (pos)
        {
            case "left":
                targetpos = -3f;
                break;
            case "center":
                targetpos = 0f;
                break;
            case "right":
                targetpos = 3f;
                break;
        }

        // Move the character to the target position
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetpos, transform.position.y, transform.position.z), horizontalSpeed * Time.deltaTime);

        // Crouching
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            startedrunning = true;
            if (tutorialcard == "crouch")
            {
                tutorial2d.transform.Find(tutorialcard).gameObject.SetActive(false);
            }
            if (isRolling == false)
            {
                crouchhitbox();
                animator.SetBool("isrolling", true);
                StartCoroutine(RollSequence());
                printCodeScript.SetCodePrompt("crouch");
            }
        }

        // Jumping
        if (isFlying == false)
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
            {
                startedrunning = true;
                if (tutorialcard == "jump")
                {
                    tutorial2d.transform.Find(tutorialcard).gameObject.SetActive(false);
                }
                if (isJumping == false)
                {
                    isJumping = true;
                    animator.SetBool("isjumping", true);
                    StartCoroutine(JumpSequence());
                }
            }
        }

        // Flying
        if (floating)
        {
            jumpedHeight = interpolateValueY(false, jumpedHeight, originY, 2.8f);
        }
        else
        {
            if (isFlying)
            {
                startY = interpolateValueY(true, startY, targetHeight, 1f);
            }
        }

        // Holding
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
        {
            tutorial2d.transform.Find("fly").gameObject.SetActive(false);
            holding = true;
            startedrunning = true;
        }
        else
        {
            holding = false;
        }

        // RAYCAST
        UpdateGroundTracking();
        if (!isJumping && !comingDown && !isFlying && !floating)
        {
            ApplyVerticalMovement();
        }
    }

    void OnTriggerEnter(Collider other)
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
                other.GetComponent<BoxCollider>().enabled = false;
                if (remainingHealth <= 0) // dead
                {
                    mainCam.GetComponent<Animator>().SetBool("dead", true);
                    isDead = true;
                    animator.Play("Stumble Backwards");
                    crashThud.Play();
                    HideAllTutorialCards();
                    levelControl.GetComponent<EndRunSequence>().enabled = true;
                    RemoveHeartsInReverseOrder();
                    this.enabled = false; // Disable this script

                }
                else if (hit == true && remainingHealth > 0) // hurt
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
            if (coinFX.pitch < 2)
            {
                coinFX.pitch += 0.2f;
            }
            else
            {
                coinFX.pitch = 1;
            }
            StartCoroutine(PitchShiftTimeout());
            CollectableControl.coinCount += 1;
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.CompareTag("powerup") || (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            printCodeScript.SetCodePrompt("fly");
            //fly object            
            godmode = true;
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 2, targetVolume = 0));
            flyFX.Play();
            Debug.Log("is playing: " + flyFX.isPlaying);
            BGM.pitch += 0.5f;
            animator.SetBool("isflying", true);
            mainCam.GetComponent<Animator>().SetBool("flying", true);
            if (!isFlying)
            {
                // Create array of coins
                // Calculate currentZ based on the relative position of the player to the map
                float currentZ = MAP.transform.InverseTransformPoint(this.transform.position).z + 230;
                //Debug.Log(currentZ);
                for (int i = 0; i < flycoinsamount; i++)
                {
                    GameObject newcoin = Instantiate(flycoin, Vector3.zero, Quaternion.identity);
                    newcoin.transform.localPosition = new Vector3(this.transform.position.x, targetHeight, currentZ + (i * 3));
                    newcoin.transform.SetParent(MAP.transform, false);
                    // Add new coin to the list
                    instantiatedCoins.Add(newcoin);
                }
                StartCoroutine(FlyTimeout());
            }
            isFlying = true;
        }

        if (other.gameObject.CompareTag("Trigger"))
        {
            Debug.Log("BRDIGE TRIGGER");
            foreach (Transform child in triggeredObject.transform)
            {
                float carRandom = Random.value;
                if (carRandom >= 0.5f)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }

        if (other.gameObject.CompareTag("pyramids") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying)
        {
            pyramidsTheme.Play();
        }

        if (other.gameObject.CompareTag("cogfactory") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying)
        {
            cogFactorySFX.Play();
        }

        if (other.gameObject.CompareTag("cogsfarm") && !mainTheme.isPlaying)
        {
            cogsfarmSFX.Play();
        }

        if (other.gameObject.CompareTag("photos") && !mainTheme.isPlaying && !photosSFX.isPlaying)
        {
            photosSFX.Play();
        }

        if (other.gameObject.CompareTag("backdoor") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying)
        {
            backDoorSFX.Play();
        }

        if (other.gameObject.CompareTag("panoptic"))
        {
            float random = Random.value;
            if (alreadyCrossedPanoptic == false)
            {
                StartCoroutine(ApplyGlissando());
                alreadyCrossedPanoptic = true;
            }
            else if (alreadyCrossedPanoptic == true)
            {
                if (random >= 0.5f)
                {
                    StartCoroutine(ApplyGlissando());
                }
            }

            if (!mainTheme.isPlaying && !panopticSFX.isPlaying && !canyonSFX.isPlaying && !pyramidsTheme.isPlaying)
            {
                panopticSFX.Play();
            }
        }

        if (other.gameObject.CompareTag("canyon") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying && !canyonSFX.isPlaying)
        {
            canyonSFX.Play();
        }
        
        if (other.gameObject.CompareTag("claxon"))
        {
            claxonSFX.Play();
        }

        if (other.gameObject.CompareTag("car") && !godmode)
        {
                other.GetComponent<BoxCollider>().enabled = false;
                mainCam.GetComponent<Animator>().SetBool("dead", true);
                animator.Play("Stumble Backwards");
                carCrashSFX.Play();
                HideAllTutorialCards();
                levelControl.GetComponent<EndRunSequence>().enabled = true;
                this.enabled = false; // Disable this script
        }

        if (other.gameObject.CompareTag("tutorial"))
        {
            HideAllTutorialCards();
            // Get the tutorial card name
            tutorialcard = other.gameObject.name;

            // Assuming tutorial2d is a Transform, find a child and set it active
            Transform tutorialCardTransform = tutorial2d.transform.Find(tutorialcard);
            if (tutorialCardTransform != null)
            {
                tutorialCardTransform.gameObject.SetActive(true);

                // Display the corresponding instruction
                if (tutorialInstructions.TryGetValue(tutorialcard, out string instruction))
                {
                    DisplayInstruction(instruction);
                }
                else
                {
                    Debug.LogError("Instruction not found for tutorial card: " + tutorialcard);
                }
            }
            else
            {
                Debug.LogError("Tutorial card not found: " + tutorialcard);
            }
        }
    }

    private void DisplayInstruction(string instruction)
    {
        tutorialText.GetComponent<Text>().text = instruction;
        tutorialText.SetActive(true);
    }

    public void SetConstrainedPositions(bool left, bool center, bool right)
    {
        blockLeft = left;
        blockCenter = center;
        blockRight = right;
    }

    void normalhitbox()
    {
        // Set new size
        boxCollider.size = new Vector3(0.67f, 1.15f, 0.58f);
        // Set new center position
        boxCollider.center = new Vector3(0, 0, -0.42f);
    }

    void jumphitbox()
    {
        // Set new center position
        boxCollider.center = new Vector3(0, 1.15f, -0.42f);
        // Set new size
        boxCollider.size = new Vector3(0.67f, 0.78f, 0.58f);
    }

    void crouchhitbox()
    {
        // Set new center position
        boxCollider.center = new Vector3(0, -0.22f, -0.42f);
        // Set new size
        boxCollider.size = new Vector3(0.67f, 0.24f, 0.58f);
    }

    IEnumerator JumpSequence()
    {
        printCodeScript.SetCodePrompt("jumpsequence");
        jumphitbox();
        yield return new WaitForSeconds(0.30f);
        comingDown = true;
        yield return new WaitForSeconds(0.30f);
        isJumping = false;
        comingDown = false;
        animator.SetBool("isjumping", false);
        normalhitbox();
    }

    IEnumerator RollSequence()
    {
        printCodeScript.SetCodePrompt("rollsequence");
        yield return new WaitForSeconds(0.45f);
        standingUp = true;
        yield return new WaitForSeconds(0.45f);
        isRolling = false;
        animator.SetBool("isrolling", false);
        normalhitbox();
    }

    IEnumerator HurtSequence()
    {
        boxCollider.enabled = false;
        yield return new WaitForSeconds(0.30f);
        boxCollider.enabled = true;
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("ishurt", false);
    }

    IEnumerator FlyTimeout()
    {
        yield return new WaitForSeconds(3);
        tutorial2d.transform.Find("fly").gameObject.SetActive(true);

        yield return new WaitForSeconds(5);
        tutorial2d.transform.Find("fly").gameObject.SetActive(false); //show tutorial

        while (holding)
        {
            yield return new WaitForSeconds(1);
        }
        mainCam.GetComponent<Animator>().SetBool("flying", false);
        StartCoroutine(delayedGodmodeOff());
        StartCoroutine(ChangePitchOverTime());
        floating = true;
        animator.SetBool("isflying", false);
        jumpedHeight = this.transform.position.y;
        yield return new WaitForSeconds(1);

        isFlying = false;
        startY = originY;

        yield return new WaitForSeconds(1);
        floating = false;
        //set move speed back to initial
        moveSpeed = initialmoveSpeed;

        // Destroy all instantiated coins
        foreach (GameObject coin in instantiatedCoins)
        {
            Destroy(coin);
        }

        // Clear the list
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
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 3, targetVolume = 1));
    }

    IEnumerator ApplyGlissando()
    {
        mainCam.GetComponent<Animator>().SetBool("panoptic", true);
        printCodeScript.SetCodePrompt("panoptic");

        float halfDuration = 4.0f;
        float elapsedTime = 0f;

        // Gradually increase the pitch from pitchMin to pitchMax
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;
            BGM.pitch = Mathf.Lerp(1f, 5f, t);
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = t, targetVolume = 1f));
            yield return null;
        }
        elapsedTime = 0f;

        // Gradually decrease the pitch from pitchMax back to pitchMin
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;
            BGM.pitch = Mathf.Lerp(5.5f, 1f, t);
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = t, targetVolume = 0.7f));
            yield return null;
        }
        // Ensure the pitch is reset to the minimum value at the end
        BGM.pitch = endingPitch;
        yield return new WaitForSeconds(6);
        mainCam.GetComponent<Animator>().SetBool("panoptic", false);
    }

    IEnumerator PlayMainTheme()
    {
        if (!mainTheme.isPlaying && !pyramidsTheme.isPlaying && !flyFX.isPlaying)
        {
            yield return new WaitForSeconds(18);
            mainTheme.Play();
            mainThemeAlreadyPlaying = true;
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeSFX", duration = 2, targetVolume = 0f)); //experimental
            yield return new WaitForSeconds(50);
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeSFX", duration = 3, targetVolume = 1f)); //experimental - turn volume up again
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

    /*public void SetConstrained(bool value) // Function to receive a bool from another script (Constrained.cs)
    {
        constrained = value;
        Debug.Log("Player constraint set to: " + constrained);
    }*/

    private float interpolateValueY(bool easingOut = true, float origin = 0.0f, float target = 5.0f, float intspeed = 0.2f)
    {
        float fraction = Time.deltaTime * intspeed;

        // Check the type of easing
        if (easingOut)
        {
            // Easing out: movement starts quickly and slows down
            fraction = 1 - Mathf.Pow(1 - fraction, 3);
            if (moveSpeed < 30 - 0f)
            {
                moveSpeed += fraction * 10;
            }
        }
        else
        {
            // Easing in: movement starts slowly and speeds up
            fraction = Mathf.Pow(fraction, 0.9f); // Adjust the power to make the easing smoother (the smaller the faster)
            if (moveSpeed > 12.0f)
            {
                moveSpeed -= fraction * 2;
            }
        }

        // Interpolate based on the currentY and targetY using the calculated fraction
        float currentY = Mathf.Lerp(origin, target, fraction);

        // Update the GameObject's position
        transform.position = new Vector3(transform.position.x, currentY, transform.position.z);
        origin = currentY;

        return origin;
    }

    private void HideAllTutorialCards()
    {
        tutorialText.SetActive(false);
        // Iterate over all direct children of the tutorial2D GameObject
        foreach (Transform child in tutorial2d.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // RAYCAST
    void UpdateGroundTracking()
    {
        // Calculate the bottom of the collider
        float feetOffset = transform.position.y + boxCollider.center.y - (boxCollider.size.y / 2f);

        // Cast the ray from a bit above the feet
        Vector3 rayOrigin = new Vector3(transform.position.x, feetOffset + raycastHeightOffset, transform.position.z);
        Ray ray = new Ray(rayOrigin, Vector3.down);
        //Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, groundLayer))
        {
            //Debug.Log($"[Raycast] Hit: {hit.collider.name}, Tag: {hit.collider.tag}");
            isGrounded = true;

            // New groundY calculation based on dynamic collider
            float groundY = hit.point.y + (boxCollider.size.y / 2f) - boxCollider.center.y;

            Vector3 pos = transform.position;
            pos.y = groundY;
            transform.position = pos;
            verticalVelocity = 0f;
        }
        else
        {
            isGrounded = false;
        }
    }

    void ApplyVerticalMovement()
    {
        if (!isGrounded)
        {
            verticalVelocity -= fallSpeed * Time.deltaTime;
            isFalling = true;
            animator.SetBool("isfalling", true);    // jumping down animation
            Vector3 pos = transform.position;
            pos.y += verticalVelocity * Time.deltaTime;
            transform.position = pos;
        }
        else
        {
            verticalVelocity = 0f;
            if (isFalling)
            {
                animator.SetBool("isfalling", false);   //back to running
                isFalling = false;
            }
        }
    }
}