using System.Collections;
using UnityEngine;

public class EndlessFall : MonoBehaviour
{
    private float decelerationRate = 1000f;
    public PlayerMove player;
    public GameObject levelControl;
    private GenerateLevel generateLevel;
    public Animator playerAnimator;
    public CollectableControl collectableControl;

    private bool isFalling = false;

    private void Start()
    {
        collectableControl = FindObjectOfType<CollectableControl>();
        generateLevel = levelControl.GetComponent<GenerateLevel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (generateLevel.inMine) return;

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
        playerAnimator.SetTrigger("endlessfall");
        collectableControl.HandlePlayerDeath();
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