using UnityEngine;

public class DestroyOnOverlap : MonoBehaviour
{

    //EXPERIMENTAL SCRIPT: DOES IT WORK??
    public string[] excludeTags = { "Player" };
    public Transform chunkRoot; // Set this to the root of the chunk this destroyer belongs to

    private void OnTriggerEnter(Collider other)
    {
        // Ignore self
        if (other.transform == transform || other.transform.IsChildOf(chunkRoot)) return;

        // Ignore excluded tags
        foreach (string tag in excludeTags)
        {
            if (other.CompareTag(tag)) return;
        }

        // Destroy the overlapping object
        Destroy(other.gameObject);
        Debug.Log("Destroyed: ");
        Debug.Log(other.gameObject);
    }
}