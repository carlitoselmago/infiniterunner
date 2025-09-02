using UnityEngine;
using System.Collections;

public class Minecart : MonoBehaviour
{
    public PlayerMove player;
    public Animator playerAnimator;
    private Rigidbody playerRb;
    public float rideLength = 500f;

    public GameObject nonAnimatedMinecart;
    public GameObject animatedMinecart;
    public GameObject MAP;

    public AudioSource minecartSFX; // experimental


    private void Start()
    {
        playerRb = player.GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerAnimator.SetBool("isdrivingminecart", true);
            animatedMinecart.SetActive(true);
            playerRb.constraints = RigidbodyConstraints.FreezeRotationX /*| RigidbodyConstraints.FreezeRotationZ*/; //or RigidbodyConstraints.None;
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

        if (accelerate)
        {
            // --- Phase 2: Stay in cart for x seconds ---
            yield return new WaitForSeconds(8.4f);

            // --- Phase 3: Decelerate back ---
            yield return StartCoroutine(ChangeSpeed(false));
            playerAnimator.SetBool("isdrivingminecart", false);
            playerAnimator.SetTrigger("jumpoffminecart");
            PlayerMove.onMinecart = false;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation; //freeze again rigidbody rotation
            animatedMinecart.transform.SetParent(MAP.transform, true); //leave cart behind
        }
    }
}
