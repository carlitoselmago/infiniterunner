using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public bool godmode = false;
    public int flycoinsamount = 30;

    private List<GameObject> instantiatedCoins = new List<GameObject>();

    public GameObject godmodevisual;
    public GameObject playerObject;
    public Rigidbody playerBody;
    private Animator animator;

    public GameObject mainCam;

    //sfx
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

    private BoxCollider boxCollider;

    private float targetHeight = 17.0f;
    private float startY;
    private float originY;
    private float jumpedHeight;

    private float speed = 2.0f; // Adjusted speed for a more natural feel

    private float elapsedTime = 0.0f; // Track time since the start of the jump

    public GameObject flycoin;

    public GameObject tutorial2d;
    public AudioSource coinFX;
    public GameObject objectWithMoveScript;

    public GameObject MAP;

    public float horizontalSpeed = 20f;

    public string pos = "center";
    private float targetpos = 0f;

    public static bool startedrunning = false;

    private string tutorialcard = "";


    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        startY = transform.position.y;
        originY = startY;
        normalhitbox();
        BGM.pitch = 1.0f;
        HideAllTutorialCards();
        startedrunning = false;
        godmodevisual.SetActive(false);
        initialmoveSpeed = moveSpeed;
    }

    void Update()

    {
        if (startedrunning == false && Input.anyKey == true)
        {
            BGM.Play();
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeBGM", duration = 3, targetVolume = 1));
            StartCoroutine(PlayMainTheme());
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 1.5f, targetVolume = 1));
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
                    pos = "left";
                }
                else if (pos == "right") // Pressing left when at right should go to center
                {
                    pos = "center";
                }
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
                    pos = "right";
                }
                else if (pos == "left") // Pressing right when at left should go to center
                {
                    pos = "center";
                }
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
        // Ajupir-se
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
                //transform.Translate(Vector3.up * Time.deltaTime * moveSpeed *-1);
                //playerObject.GetComponent<Animator>().Play("Quick Roll To Run");
                animator.SetBool("isrolling", true);
                StartCoroutine(RollSequence());
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
                    //playerObject.GetComponent<Animator>().Play("Jump");
                    animator.SetBool("isjumping", true);
                    StartCoroutine(JumpSequence());
                }
            }
        }
        else
        { }

        if (isJumping == true)
        {
            if (comingDown == false)
            {
                //transform.Translate(Vector3.up * Time.deltaTime * 4.5f, Space.World);
            }
            else
            {
                //transform.Translate(Vector3.up * Time.deltaTime * -4.5f, Space.World);
            }
        }

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
        {
            //hold
            tutorial2d.transform.Find("fly").gameObject.SetActive(false);
            holding = true;
            startedrunning = true;
        }
        else
        {
            holding = false;
        }

        // fly
        if (floating)
        {
            // PerformFall();
            jumpedHeight = interpolateValueY(false, jumpedHeight, originY, 2.8f);
        }
        else
        {
            if (isFlying)
            {
                // PerformFly();
                startY = interpolateValueY(true, startY, targetHeight, 1f);
                
                //Debug.Log(startY);
                //Debug.Log(targetHeight);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("obstacle"))
        {
            if (!godmode)
                {
                    Debug.Log("Entered in collision with " + other);
                    other.GetComponent<BoxCollider>().enabled = false;
                    mainCam.GetComponent<Animator>().SetBool("dead", true);
                    animator.Play("Stumble Backwards");
                    crashThud.Play();
                    //mainCam.GetComponent<Animator>().enabled = true;
                    HideAllTutorialCards();
                    levelControl.GetComponent<EndRunSequence>().enabled = true;
                    // Disable this script
                    this.enabled = false;
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
            //fly object
            //Debug.Log("FLY COLLISION!!!!!!!!!!!!!!!!!!!!");
            
            godmode = true;
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 2, targetVolume = 0));
            flyFX.Play();
            BGM.pitch += 0.5f;
            animator.SetBool("isflying", true);
            mainCam.GetComponent<Animator>().SetBool("flying", true);
            if (!isFlying)
            {
                //create array of coins
                 // Calculate currentZ based on the relative position of the player to the map
                float currentZ = MAP.transform.InverseTransformPoint(this.transform.position).z + 230;
                Debug.Log(currentZ);
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

        if (other.gameObject.CompareTag("panoptic") && !mainTheme.isPlaying && !panopticSFX.isPlaying && !canyonSFX.isPlaying && !pyramidsTheme.isPlaying)
        {
            panopticSFX.Play();
        }

        if (other.gameObject.CompareTag("canyon") && !mainTheme.isPlaying && !pyramidsTheme.isPlaying && !canyonSFX.isPlaying)
        {
            canyonSFX.Play();
        }

        if (other.gameObject.CompareTag("claxon"))
        {
            claxonSFX.Play();
        }

        if (other.gameObject.CompareTag("car"))
        {
            if (!godmode)
            {
                    other.GetComponent<BoxCollider>().enabled = false;
                    mainCam.GetComponent<Animator>().SetBool("dead", true);
                    animator.Play("Stumble Backwards");
                    carCrashSFX.Play();
                    HideAllTutorialCards();
                    levelControl.GetComponent<EndRunSequence>().enabled = true;
                    // Disable this script
                    this.enabled = false;
            }
        }

        if (other.gameObject.CompareTag("tutorial"))
        {
            // Hide all previous tutorial panels
            HideAllTutorialCards();
            // Get the tutorial card name
            tutorialcard = other.gameObject.name;

            // Assuming tutorial2d is a Transform, find a child and set it active
            Transform tutorialCardTransform = tutorial2d.transform.Find(tutorialcard);
            if (tutorialCardTransform != null)
            {
                tutorialCardTransform.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Tutorial card not found: " + tutorialcard);
            }
        }

    }

    void normalhitbox()
    {
        // Set new size
        boxCollider.size = new Vector3(0.67f, 1, 0.58f);
        // Set new center position
        boxCollider.center = new Vector3(0, 0, -0.42f);
    }

    void jumphitbox()
    {
        // Set new center position
        boxCollider.center = new Vector3(0, 1, -0.42f);
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
        yield return new WaitForSeconds(0.45f);
        standingUp = true;
        yield return new WaitForSeconds(0.45f);
        isRolling = false;
        animator.SetBool("isrolling", false);
        normalhitbox();
    }

    IEnumerator FlyTimeout()
    {
        //Debug.Log("FLYTIMEOUT!!!!!!!!!!!!");

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
        moveSpeed=initialmoveSpeed ;

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

        // Ensure the pitch is exactly what we want at the end
        BGM.pitch = endingPitch;
        StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 3, targetVolume = 1));
    }

    IEnumerator PlayMainTheme()
    {
        if (!mainTheme.isPlaying && !pyramidsTheme.isPlaying && !flyFX.isPlaying)
        {
            yield return new WaitForSeconds(17);
            mainTheme.Play();
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

    private void PerformFly()
    {
        float tweenspeed = 1.0f;
        // Calculate the new Y position with easing out effect
        float newY = Mathf.Lerp(startY, targetHeight, 1 - Mathf.Pow(1 - Time.deltaTime * tweenspeed, 3));

        // Apply the new Y position
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Optionally update startY to continue the movement from the current position
        startY = newY;
    }

    private void PerformFall()
    {
        elapsedTime += Time.deltaTime;
        float timeFraction = elapsedTime / (5); // Time fraction over the total duration

        if (timeFraction < 2f)
        {
            // Ease out effect using Mathf.SmoothStep for smoother transition
            float initialY = jumpedHeight;//startY + jumpedHeight; // Start falling from this height

            float newY = Mathf.SmoothStep(initialY, startY, timeFraction);

            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            moveSpeed -= 0.01f; // Increase move speed (assuming it's needed for your context)
            Debug.Log("falling............");
        }
        else
        {
            // Ensure it ends exactly at the startY and stop the fall
            transform.position = new Vector3(transform.position.x, startY, transform.position.z);
            //isFlying = false; // Consider renaming this flag to better suit your context, like isFalling or movementFinished
            
        }
    }

    private void HideAllTutorialCards()
    {
        // Iterate over all direct children of the tutorial2D GameObject
        foreach (Transform child in tutorial2d.transform)
        {
            // Disable the child GameObject
            child.gameObject.SetActive(false);
        }
    }
}