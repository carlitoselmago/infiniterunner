using UnityEngine;

public class DisableOnOverlap : MonoBehaviour, IResettable
{
    [Tooltip("Tag of the colliding object that will disable this GameObject.")]
    public string obstacleTag = "obstacle";

    private int hasSoundLayer;

    private void Awake()
    {
        hasSoundLayer = LayerMask.NameToLayer("HasSound");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldDisable(other.gameObject))
            gameObject.SetActive(false);
    }

    private bool ShouldDisable(GameObject obj)
    {
        // Ignore self or colliders that are part of this object
        if (obj == gameObject || obj.transform.IsChildOf(transform))
            return false;

        // Only disable if tagged obstacle AND NOT on hasSound layer
        if (obj.CompareTag(obstacleTag) && obj.layer != hasSoundLayer)
            return true;

        return false;
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
    }
}
