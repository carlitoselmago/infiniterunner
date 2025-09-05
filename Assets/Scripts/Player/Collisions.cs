/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// INCORRECT REMOVE
public class Collisions : MonoBehaviour
{




    public CollectableControl collectableControl;
    public BoxCollider boxCollider;
    public BoxCollider jumpCollider;

    public HurtMask hurtMaskScript;


    [Header("Health")]
    public static int maxHealth = 5;
    public static int remainingHealth;
    private bool hit = false;
    public static bool isDead = false;
    public bool godmode = false;
    public int flycoinsamount = 30;

    private List<GameObject> instantiatedCoins = new List<GameObject>();

    public GameObject godmodevisual;
    public GameObject playerObject;
    public Rigidbody playerBody;

    public GameObject hearts;
    public GameObject heart;
    // List to store instantiated hearts
    public List<GameObject> heartList = new List<GameObject>();



    void Start()
    {
        BoxCollider[] colliders = GetComponents<BoxCollider>();

        if (colliders.Length >= 3)
        {
            boxCollider = colliders[0];
            jumpCollider = colliders[1];
            BoxCollider PhysicsCollider = colliders[2];
        }
        else
        {
            Debug.LogError("Player needs two BoxColliders: one for standing, one for jumping!");
        }
        collectableControl = FindObjectOfType<CollectableControl>();

        isDead = false;
        remainingHealth = 0;
        godmodevisual.SetActive(false);

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
        Destroy(heartList[lastindex]);

        // Remove the heart from the list
        heartList.RemoveAt(lastindex);
    }

        void OnTriggerEnter(Collider other)
        {
            //HideAllTutorialCards();
            if (other.gameObject.CompareTag("obstacle"))
            {
                if (!godmode)
                {
                    hit = true;
                    //printCodeScript.SetCodePrompt("dead");
                    StartCoroutine(hurtMaskScript.Mask());
                    remainingHealth--;
                    Debug.Log("Entered in collision with " + other);
                    other.GetComponent<BoxCollider>().enabled = false;

                    if (remainingHealth <= 0)
                    {
                        collectableControl.HandlePlayerDeath();
                        //mainCam.GetComponent<Animator>().SetBool("dead", true);
                        isDead = true;
                        //animator.Play("Stumble Backwards");
                        //crashThud.Play();
                        //levelControl.GetComponent<GenerateSandstorm>().enabled = false;
                        //HideAllTutorialCards();
                        //StartCoroutine(EnableEndSequenceSafely());
                        //levelControl.GetComponent<EndRunSequence>().enabled = true;
                        RemoveHeartsInReverseOrder();
                        this.enabled = false;
                    }

                    else if (hit && remainingHealth > 0) // hurt
                    {
                        //printCodeScript.SetCodePrompt("hurt");
                       // animator.SetBool("ishurt", true);
                        StartCoroutine(HurtSequence());
                        //HurtSFX.Play();
                        RemoveHeartsInReverseOrder();
                    }
                    hit = false;
                }
            }

            if (other.gameObject.CompareTag("coin"))
            {
                //coinFX.pitch = 1;
                //coinFX.Play();
                CollectableControl.coinCount += 1;
                other.gameObject.SetActive(false);
            }
            /*
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

            if (other.gameObject.CompareTag("powerup") || (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
            {
                printCodeScript.SetCodePrompt("fly");
                //fly object            
                godmode = true;
                StartCoroutine(FadeMixerGroup.StartFade(audioMixer, exposedParameter = "volumeThemes", duration = 2, targetVolume = 0));
                flyFX.Play();
                BGM.pitch += 0.5f;
                animator.SetBool("isflying", true);
                mainCam.GetComponent<Animator>().SetBool("flying", true);
                if (!isFlying)
                {
                    // Create array of coins
                    // Calculate currentZ based on the relative position of the player to the map
                    float currentZ = MAP.transform.InverseTransformPoint(this.transform.position).z + 230;
                    for (int i = 0; i < flycoinsamount; i++)
                    {
                        GameObject newcoin = Instantiate(flycoin, Vector3.zero, Quaternion.identity);
                        newcoin.transform.localPosition = new Vector3(this.transform.position.x, targetHeight, currentZ + (i * 3));
                        newcoin.transform.SetParent(MAP.transform, false);
                        instantiatedCoins.Add(newcoin);
                    }
                    StartCoroutine(FlyTimeout());
                }
                isFlying = true;
                playerBody.isKinematic = true;
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

            if (other.gameObject.CompareTag("cardboard"))
                (Random.value < 0.5f ? cardboard1 : cardboard2).Play();

            if (other.gameObject.CompareTag("panoptic"))
            {
                if (!alreadyCrossedPanoptic)
                {
                    StartCoroutine(ApplyGlissando());
                    alreadyCrossedPanoptic = true;
                }
                else if (alreadyCrossedPanoptic)
                {
                    if (Random.value >= 0.5f)
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
                collectableControl.HandlePlayerDeath();
                StartCoroutine(EnableEndSequenceSafely());
                this.enabled = false; // Disable this script
            }

            if (other.gameObject.CompareTag("minewall"))        //remove if not used
            {
                other.GetComponent<BoxCollider>().enabled = false;
                mainCam.GetComponent<Animator>().SetBool("dead", true);
                animator.Play("Stumble Backwards");
                //carCrashSFX.Play();       // Replace by stone SFX
                Transform child = playerObject.transform.Find("rocks");
                child.gameObject.SetActive(true);
                HideAllTutorialCards();
                collectableControl.HandlePlayerDeath();
                StartCoroutine(EnableEndSequenceSafely());
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
            }
        }


    IEnumerator HurtSequence()
    {
        boxCollider.enabled = false;
        yield return new WaitForSeconds(0.3f);
        boxCollider.enabled = true;
        yield return new WaitForSeconds(0.5f);
        //animator.SetBool("ishurt", false);
    }






}
*/