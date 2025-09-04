using UnityEngine;
using System.Collections;

public class Minecart : MonoBehaviour
{
    public PlayerMove player;
    public Animator playerAnimator;
    private Rigidbody playerRb;
    public float rideLength = 500f;

    public GameObject nonAnimatedMinecart;
    public GameObject animatedMinecartPrefab;
    public GameObject MAP;
    public AudioSource minecartSFX;
    private Transform minecartHolder;
    // Define offsets
    Vector3 positionOffset = new Vector3(-0.012f, -0.129f, -0.58f);
    Quaternion rotationOffset = Quaternion.Euler(0, 270, 0);

    private bool triggered = false;

    private void Start()
    {
        playerRb = player.GetComponent<Rigidbody>();

        // Find the "minecart" holder object under the player
        minecartHolder = player.transform.Find("minecart");
        if (minecartHolder == null)
            Debug.LogError("Minecart holder not found under player! Please create a child named 'minecart'.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            triggered = true;
            minecartSFX.Play();
            playerAnimator.SetBool("isdrivingminecart", true);

            // --- Spawn a working copy ---
            GameObject rideCart = Instantiate(animatedMinecartPrefab, minecartHolder);
            rideCart.transform.localPosition = positionOffset;
            rideCart.transform.localRotation = rotationOffset;
            rideCart.SetActive(true);

            playerRb.constraints = RigidbodyConstraints.FreezeRotationX /*| RigidbodyConstraints.FreezeRotationZ*/; //or RigidbodyConstraints.None;
            StartCoroutine(ChangeSpeed(true, rideCart));
            nonAnimatedMinecart.SetActive(false);
            PlayerMove.onMinecart = true;
        }
    }

    IEnumerator ChangeSpeed(bool accelerate, GameObject rideCart)
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

        if (accelerate)
        {
            // --- Phase 2: Stay in cart for x seconds ---
            yield return new WaitForSeconds(8.4f);

            // --- Phase 3: Decelerate back ---
            yield return StartCoroutine(ChangeSpeed(false, rideCart));
            playerAnimator.SetBool("isdrivingminecart", false);
            playerAnimator.SetTrigger("jumpoffminecart");
            PlayerMove.onMinecart = false;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation; //freeze again rigidbody rotation
            rideCart.transform.SetParent(MAP.transform, true); //leave cart behind
            yield return new WaitForSeconds(4);
            Destroy(rideCart);
        }
    }
}