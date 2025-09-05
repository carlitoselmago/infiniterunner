using UnityEngine;

public class Constrain : MonoBehaviour
{
    private PlayerMove player;

    // Set which positions are constrained in the Inspector
    public bool constrainLeft = false;
    public bool constrainCenter = false;
    public bool constrainRight = false;

    void Start()
    {
       GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerMove>();
            if (player == null)
                Debug.LogError("Constrain: Player object found but no PlayerMove script attached.");
        }
        else
            Debug.LogError("Constrain: No GameObject with tag 'Player' found in the scene.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && player != null)
            player.SetConstrainedPositions(constrainLeft, constrainCenter, constrainRight);
    }
}