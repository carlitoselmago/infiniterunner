using System.Collections;
using UnityEngine;

public class EndlessFall : MonoBehaviour, IResettable
{
    private float decelerationRate = 800f;
    public PlayerMove player;
    public GameObject levelControl;
    public Animator playerAnimator;
    public CollectableControl collectableControl;

    public bool overrideMine = false;   // set true for deep falling trigger (fallback in case the player accidentally sinks through ground)
    private bool isFalling = false;
    private bool triggered = false;

    private void Start()
    {
        if (collectableControl == null)
            collectableControl = FindObjectOfType<CollectableControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (MineData.endlessFallDisabled && !overrideMine) return;

        if (!isFalling && other.CompareTag("Player") && !triggered)
        {
            //Debug.Log("Endlessly falling!");
            triggered = true;
            isFalling = true;
            StartCoroutine(HandleEndlessFall());
        }
    }

    private IEnumerator HandleEndlessFall()
    {
        playerAnimator.SetTrigger("endlessfall");
        playerAnimator.SetBool("isrunning", false);
        playerAnimator.SetBool("isfalling", false);

        //Debug.Log("Handling endless fall...");

        if (!MineData.isInTheMine) // Allow the player to hit ground when falling from the minecart
            PlayerMove.isUnderwater = true; // Disable physics collider to prevent collisions with Ground while falling
        else
            collectableControl.HandlePlayerDeath(); // otherwise managed by PlayerMove if isUnderwater is true

        levelControl.GetComponent<EndRunSequence>().enabled = true;

        // Gradually reduce movement speed
        while (player.moveSpeed > 0.01f)
        {
            player.moveSpeed = Mathf.MoveTowards(player.moveSpeed, 0, decelerationRate * Time.deltaTime);
            yield return null;
        }
        player.moveSpeed = 0;
    }

    public void ResetState()
    {
        triggered = false;
        isFalling = false;
    }
}