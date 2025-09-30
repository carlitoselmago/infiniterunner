using UnityEngine;
using System.Collections;
using System.Linq;

public class Forklift : MonoBehaviour, IResettable
{
    public PlayerMove player;
    public Transform playerCharacter;
    public Animator playerAnimator;
    private Rigidbody playerRb;
    private Collider[] playerColliders;

    public GameObject nonAnimatedForklift;
    public GameObject animatedForkliftPrefab;
    public GameObject MAP;
    public AudioSource forkliftSFX;

    private GameObject rideForklift;
    private Transform forkliftHolder;
    private Rigidbody forkliftRb;
    private Collider[] forkliftColliders;

    [Header("Fork Controls")]
    private Transform forkTransform;
    public float forkSpeed = 1.5f;   // units per second
    public float minForkY = 1.5f;      // bottom local Y
    public float maxForkY = 8.5f;    // top local Y
    private Rigidbody forkRb;


    // Offsets (adjust as needed)
    Vector3 positionOffset = new Vector3(0f, 1f, -0.5f);
    Quaternion rotationOffset = Quaternion.Euler(0, 270, 0);

    private bool triggered = false;

    private void Start()
    {
        playerRb = player.GetComponent<Rigidbody>();
        playerColliders = player.GetComponentsInChildren<Collider>();

        forkliftHolder = player.transform.Find("forklift");
        if (forkliftHolder == null)
            Debug.LogError("Forklift holder not found under player! Please create a child named 'forklift'.");
    }

    void OnEnable()
    {
        if (!nonAnimatedForklift.activeSelf)
            nonAnimatedForklift.SetActive(true);
    }
    
    void Update()
    {
        if (PlayerMove.onForklift)
        {
            if (PlayerMove.isDead)
            {
                rideForklift.GetComponent<Explodable>().enabled = true;
            }

            if (forkTransform != null)
            {
                Vector3 pos = forkTransform.localPosition;

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    pos.z += forkSpeed * Time.deltaTime;
                    Debug.Log("up");
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    pos.z -= forkSpeed * Time.deltaTime;
                    Debug.Log("down");
                }

                pos.z = Mathf.Clamp(pos.z, minForkY, maxForkY);

                forkTransform.localPosition = pos;
            }
        }
    }






    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            triggered = true;
            forkliftSFX.Play();

            playerAnimator.SetBool("isdrivingminecart", true);

            // Raise the player body smoothly
            player.StartCoroutine(player.RaisePlayerBody(0.511f, 0.6f));

            // Spawn forklift *under forkliftHolder*
            rideForklift = Instantiate(animatedForkliftPrefab, forkliftHolder.position, animatedForkliftPrefab.transform.rotation);
            rideForklift.transform.SetParent(forkliftHolder, true);  // true = keep world position/rotation

            rideForklift.SetActive(true);
            rideForklift.GetComponent<Animator>().enabled = true;

            // Grab forklift physics
            forkliftRb = rideForklift.GetComponent<Rigidbody>();
            forkliftColliders = rideForklift.GetComponentsInChildren<Collider>();

            // Assign fork rigidbody for movement
            forkTransform = rideForklift.GetComponentsInChildren<Transform>()
                            .FirstOrDefault(t => t.name == "VisMast");
            if (forkTransform != null)
            {
                forkRb = forkTransform.GetComponent<Rigidbody>();
                if (forkRb == null)
                    forkRb = forkTransform.gameObject.AddComponent<Rigidbody>();
                forkRb.isKinematic = true;  // controlled by script

                Debug.Log("Fork ready");
            } else
            {
                Debug.Log("No Fork found");
            }

            // Switch to forklift physics
            //TakeOverPhysics();

            PlayerMove.onForklift = true;
            player.forkliftManager = this;

            StartCoroutine(SetForkliftSpeed(8f));

            nonAnimatedForklift.SetActive(false);
        }
    }


    //not used
    void TakeOverPhysics()
    {
        // Disable player physics
        if (playerRb != null)
            playerRb.isKinematic = true;
        foreach (var col in playerColliders)
            col.enabled = false;

        // Enable forklift physics
        if (forkliftRb != null)
            forkliftRb.isKinematic = false;
        if (forkliftColliders != null)
        {
            foreach (var col in forkliftColliders)
                col.enabled = true;
        }
    }
    //not used
    void RestorePlayerPhysics()
    {
        // Re-enable player physics
        if (playerRb != null)
            playerRb.isKinematic = false;
        foreach (var col in playerColliders)
            col.enabled = true;

        // Disable forklift physics so it stops interfering
        if (forkliftRb != null)
            forkliftRb.isKinematic = true;
        if (forkliftColliders != null)
        {
            foreach (var col in forkliftColliders)
                col.enabled = false;
        }
    }

    IEnumerator SetForkliftSpeed(float targetSpeed)
    {
        float duration = 1f;
        float startSpeed = player.moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            player.moveSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsed / duration);
            yield return null;
        }
        player.moveSpeed = targetSpeed;
    }

    public void ExitForklift()
    {
        // Smoothly lower player body again
        player.StartCoroutine(player.RaisePlayerBody(-0.35f, 0.6f));

        playerAnimator.SetBool("isdrivingminecart", false);
        PlayerMove.onForklift = false;
        player.moveSpeed = 12f;

        //RestorePlayerPhysics();

        // Leave forklift behind but disable it (or destroy if you want cleanup)
        StartCoroutine(LeaveForklift());
    }

    private IEnumerator LeaveForklift()
    {
        Debug.Log("Unparented Forklift");
        rideForklift.transform.SetParent(MAP.transform, true); //leave it behind
        yield return new WaitForSeconds(4);
        rideForklift.SetActive(false);
        Debug.Log("Forklift Destroyed");
    }

    public void ResetState()
    {
        Debug.Log("Resetting Forklift");
        nonAnimatedForklift.SetActive(true);
        triggered = false;

        if (forkliftHolder != null)
        {
            for (int i = forkliftHolder.childCount - 1; i >= 0; i--)
            {
                Transform child = forkliftHolder.GetChild(i);
                child.gameObject.SetActive(false);
            }
        }
    }
}