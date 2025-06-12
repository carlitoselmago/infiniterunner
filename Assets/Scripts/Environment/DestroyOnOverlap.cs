/*using UnityEngine;

public class DestroyOnOverlap : MonoBehaviour
{
    public string[] excludeTags = { "Player", "Ground" };
    public Transform chunkRoot; // Assign the root of this chunk in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        TryDestroy(other.gameObject);
    }

    // Also catch objects without colliders manually (e.g., via raycasting or overlap checks elsewhere)
    private void OnTriggerStay(Collider other)
    {
        TryDestroy(other.gameObject);
    }

    private void TryDestroy(GameObject obj)
    {
        if (obj == null) return;

        // Ignore self
        if (obj.transform == transform || obj.transform.IsChildOf(chunkRoot)) return;

        // Ignore excluded tags
        foreach (string tag in excludeTags)
        {
            if (obj.CompareTag(tag)) return;
        }

        Debug.Log($"Destroyed object: {obj.name}");
        Destroy(obj);
    }
}*/