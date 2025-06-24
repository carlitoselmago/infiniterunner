using System.Collections;
using UnityEngine;

public class EndlessFall : MonoBehaviour
{
    private float decelerationRate = 1000f;
    public PlayerMove player;
    public GameObject levelControl;
    public Animator playerAnimator;

    private bool isFalling = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isFalling && other.CompareTag("Player"))
        {
            Debug.Log("FALLING!");
            isFalling = true;
            StartCoroutine(HandleEndlessFall());
        }
    }

    private IEnumerator HandleEndlessFall()
    {
        float originalSpeed = player.moveSpeed;
        playerAnimator.SetBool("isendlesslyfalling", true);
        levelControl.GetComponent<EndRunSequence>().enabled = true;

        // Gradually reduce movement speed
        while (player.moveSpeed > 0.01f)
        {
            player.moveSpeed = Mathf.MoveTowards(player.moveSpeed, 0, decelerationRate * Time.deltaTime);
            yield return null;
        }
        player.moveSpeed = 0;
    }
}