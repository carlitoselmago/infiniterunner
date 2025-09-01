/*using UnityEngine;

public class DisableOnContact : MonoBehaviour
{
    [Tooltip("Layers to ignore (won't disable)")]
    public LayerMask ignoreLayers;

    [Tooltip("Specific GameObject (and its children) to ignore (optional)")]
    public GameObject ignoreObject;

    void OnTriggerEnter(Collider other)
    {
        // If ignoreObject is assigned, skip it and all its children
        if (ignoreObject != null && other.transform.IsChildOf(ignoreObject.transform))
            return;

        // If the object’s layer is in the ignore mask, skip it
        if ((ignoreLayers.value & (1 << other.gameObject.layer)) != 0)
            return;

        // Otherwise disable it
        other.gameObject.SetActive(false);
    }
}*/
//

using UnityEngine;

public class DisableOnContact : MonoBehaviour
{
    [Tooltip("Layers to ignore (won't disable, including their children)")]
    public LayerMask ignoreLayers;

    [Tooltip("Specific GameObject (and its children) to ignore (optional)")]
    public GameObject ignoreObject;

    void OnTriggerEnter(Collider other)
    {
        // If ignoreObject is assigned, skip it and all its children
        if (ignoreObject != null && other.transform.IsChildOf(ignoreObject.transform))
            return;

        // If this object or any of its parents is in the ignoreLayers, skip it
        Transform current = other.transform;
        while (current != null)
        {
            if ((ignoreLayers.value & (1 << current.gameObject.layer)) != 0)
                return;
            current = current.parent;
        }

        // Otherwise disable it
        other.gameObject.SetActive(false);
    }
}
