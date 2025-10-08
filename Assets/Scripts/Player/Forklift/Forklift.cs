// Forklift.cs (trigger / spawner)
using UnityEngine;
using System.Collections;

public class Forklift : MonoBehaviour, IResettable
{
    public PlayerMove player;
    public Animator playerAnimator;
    public GameObject nonAnimatedForklift;
    public GameObject animatedForkliftPrefab;
    public GameObject MAP;

    private GameObject rideForklift;   // always track the single spawned instance
    private Transform forkliftHolder;
    private bool triggered = false;

    private void Start()
    {
        if (player == null)
            Debug.LogError("Forklift: player reference is missing!");

        forkliftHolder = player.transform.Find("forklift");
        if (forkliftHolder == null)
            Debug.LogError("Forklift holder not found under player! Please create a child named 'forklift'.");
    }

    void OnEnable()
    {
        if (nonAnimatedForklift != null && !nonAnimatedForklift.activeSelf)
            nonAnimatedForklift.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || triggered) return;
        if (PlayerMove.onForklift) return;

        // Prevent double spawn if a ride exists
        if (rideForklift != null) return;

        triggered = true;
        if (!CollectableControl.firstForklift)
            CollectableControl.firstForklift = true;

        if (playerAnimator != null)
            playerAnimator.SetBool("isdrivingminecart", true);

        // Raise the player body visually
        player.StartCoroutine(player.RaisePlayerBody(0.511f, 0.6f));

        // Spawn the prefab (parented to forkliftHolder initially so it matches position/rotation)
        Vector3 spawnPos = forkliftHolder != null ? forkliftHolder.position : transform.position;
        Quaternion spawnRot = animatedForkliftPrefab != null ? animatedForkliftPrefab.transform.rotation : Quaternion.identity;

        rideForklift = Instantiate(animatedForkliftPrefab, spawnPos, spawnRot, forkliftHolder);
        rideForklift.SetActive(true);

        // Ensure prefab has RideForklift and initialize it
        RideForklift rideScript = rideForklift.GetComponent<RideForklift>();
        if (rideScript == null)
            rideScript = rideForklift.AddComponent<RideForklift>();

        // Initialize; pass playerAnimator so RideForklift can toggle anim states
        rideScript.Initialize(player, MAP, this, playerAnimator);

        // PlayerMove should keep a reference to the active RideForklift (so exit calls are consistent)
        player.forkliftManager = rideScript;
        PlayerMove.onForklift = true;

        StartCoroutine(SetForkliftSpeed(8f));

        if (nonAnimatedForklift != null)
            nonAnimatedForklift.SetActive(false);
    }

    // Called by RideForklift when that object is destroyed/disabled so we stop referencing it
    public void NotifyRideDestroyed(GameObject whichRide)
    {
        if (rideForklift == whichRide)
        {
            rideForklift = null;
            triggered = false; // allow spawning again
            if (nonAnimatedForklift != null)
                nonAnimatedForklift.SetActive(true);
        }
    }

    IEnumerator SetForkliftSpeed(float targetSpeed)
    {
        if (player == null) yield break;

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

    public void ResetState()
    {
        if (rideForklift != null)
        {
            Destroy(rideForklift);
            rideForklift = null;
        }

        if (nonAnimatedForklift != null)
            nonAnimatedForklift.SetActive(true);

        triggered = false;
    }
}
