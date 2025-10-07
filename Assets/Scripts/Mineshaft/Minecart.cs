using UnityEngine;
using System.Collections;

public class Minecart : MonoBehaviour, IResettable
{
    public PlayerMove player;
    public Animator playerAnimator;
    public float rideLength = 500f;

    public GameObject nonAnimatedMinecart;
    public GameObject animatedMinecartPrefab;
    public GameObject MAP;
    public AudioSource minecartSFX;
    private GameObject rideCart;
    private Transform minecartHolder;
    // Define offsets
    Vector3 positionOffset = new Vector3(-0.012f, -0.129f, -0.58f);
    Quaternion rotationOffset = Quaternion.Euler(0, 270, 0);

    private bool triggered = false;

    private void Start()
    {
        // Find the "minecart" holder object under the player
        minecartHolder = player.transform.Find("minecart");
        if (minecartHolder == null)
            Debug.LogError("Minecart holder not found under player! Please create a child named 'minecart'.");
    }

   void OnEnable()
    {
        if (!nonAnimatedMinecart.activeSelf)
            nonAnimatedMinecart.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            triggered = true;
            minecartSFX.Play();
            playerAnimator.SetBool("isdrivingminecart", true);

            // --- Spawn a working copy ---
            /*GameObject */rideCart = Instantiate(animatedMinecartPrefab, minecartHolder);
            rideCart.transform.localPosition = positionOffset;
            rideCart.transform.localRotation = rotationOffset;
            rideCart.SetActive(true);

            StartCoroutine(ChangeSpeed(true));
            nonAnimatedMinecart.SetActive(false);
            PlayerMove.onMinecart = true;
        }
    }

    IEnumerator ChangeSpeed(bool accelerate)
    {
        float targetSpeed = accelerate ? 40f : 12f;   // where we want to go
        float duration = 1.2f;                        // time to reach target
        float startSpeed = player.moveSpeed;          // current speed
        float elapsed = 0f;

        // --- Phase 1: Ramp speed ---
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            player.moveSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsed / duration);
            yield return null;
        }
        player.moveSpeed = targetSpeed;
        PlayerMove.rayLength = 0.25f;

        if (accelerate)
        {
            // --- Phase 2: Stay in cart for x seconds ---
            yield return new WaitForSeconds(8.4f);

            // --- Phase 3: Decelerate back ---
            PlayerMove.rayLength = 1.5f;
            yield return StartCoroutine(ChangeSpeed(false));
            playerAnimator.SetBool("isdrivingminecart", false);
            playerAnimator.SetTrigger("jumpoffminecart");
            PlayerMove.onMinecart = false;
            PlayerMove.rayLength = 1.2f;
            rideCart.transform.SetParent(MAP.transform, true); //leave cart behind
            yield return new WaitForSeconds(4);
            rideCart.SetActive(false);
        }
    }

    public void CartCrash()
    {
        rideCart.transform.SetParent(MAP.transform, true);
        Rigidbody cartRb = rideCart.GetComponent<Rigidbody>();
        BoxCollider cartCollider = rideCart.GetComponent<BoxCollider>();
        cartCollider.enabled = true;
        cartRb.isKinematic = false;
        Vector3 pushDir = transform.up;
        cartRb.AddForce(pushDir * 12f, ForceMode.Impulse);
    }

    public void ResetState()
    {
        nonAnimatedMinecart.SetActive(true);
        triggered = false;
        
        // --- Cleanup spawned carts in the holder ---
        if (minecartHolder != null)
        {
            for (int i = minecartHolder.childCount - 1; i >= 0; i--)
            {
                Transform child = minecartHolder.GetChild(i);
                child.gameObject.SetActive(false);
            }
        }
    }
}