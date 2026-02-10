using UnityEngine;

public class Skateboard : MonoBehaviour, IResettable
{
    public PlayerMove player;
    public Animator playerAnimator;
    public GameObject nonAnimatedSkateboard;
    public GameObject animatedSkateboardPrefab;
    public GameObject MAP;

    private GameObject rideSkateboard;   // always track the single spawned instance
    private Transform skateboardHolder;
    private bool triggered = false;

    private void Start()
    {
        //if (player == null)
            //Debug.LogError("Skateboard: player reference is missing!");

        skateboardHolder = player.transform.Find("skateboard");
        //if (skateboardHolder == null)
            //Debug.LogError("Skateboard holder not found under player.");
    }

    void OnEnable()
    {
        if (nonAnimatedSkateboard != null && !nonAnimatedSkateboard.activeSelf)
            nonAnimatedSkateboard.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || triggered) return;
        if (PlayerMove.onSkateboard || PlayerMove.onForklift) return;

        if (rideSkateboard != null) return;
        triggered = true;
 
        if (!CollectableControl.firstSkateboard)
            CollectableControl.firstSkateboard = true;

        if (playerAnimator != null)
            playerAnimator.SetBool("isskating", true);

        player.StartCoroutine(player.RaisePlayerBody(-0.3f, 0.6f));

        // Spawn the prefab (parented to forkliftHolder initially so it matches position/rotation)
        Vector3 spawnPos = skateboardHolder != null ? skateboardHolder.position : transform.position;
        Quaternion spawnRot = animatedSkateboardPrefab != null ? animatedSkateboardPrefab.transform.rotation : Quaternion.identity;

        rideSkateboard = Instantiate(animatedSkateboardPrefab, spawnPos, spawnRot, skateboardHolder);
        rideSkateboard.SetActive(true);

        // Ensure prefab has RideSkateboardt and initialize it
        RideSkateboard rideScript = rideSkateboard.GetComponent<RideSkateboard>();
        if (rideScript == null)
            rideScript = rideSkateboard.AddComponent<RideSkateboard>();

        // Initialize; pass playerAnimator so RideSkateboard can toggle anim states
        rideScript.Initialize(player, MAP, this, playerAnimator);

        // PlayerMove should keep a reference to the active RideSkateboard (so exit calls are consistent)
        player.skateboardManager = rideScript;
        PlayerMove.onSkateboard = true;

        if (nonAnimatedSkateboard != null)
            nonAnimatedSkateboard.SetActive(false);
    }

    // Called by RideSkateboard when that object is destroyed/disabled so we stop referencing it
    public void NotifyRideDestroyed(GameObject whichRide)
    {
        if (rideSkateboard == whichRide)
        {
            rideSkateboard = null;
            triggered = false; // allow spawning again
            if (nonAnimatedSkateboard != null)
                nonAnimatedSkateboard.SetActive(true);
        }
    }

    public void ResetState()
    {
        if (rideSkateboard != null)
        {
            Destroy(rideSkateboard);
            rideSkateboard = null;
        }

        if (nonAnimatedSkateboard != null)
            nonAnimatedSkateboard.SetActive(true);

        triggered = false;
    }
}