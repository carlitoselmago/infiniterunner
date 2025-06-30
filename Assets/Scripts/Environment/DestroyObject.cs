using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [Tooltip("The object that will be destroyed if overlapped")]
    public GameObject destroyedObject;

    void Start()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, transform.localScale / 2, Quaternion.identity);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == destroyedObject)
            {
                Destroy(destroyedObject);
                Debug.Log($"Object destroyed at spawn: {destroyedObject.name}");
            }
        }
    }
}