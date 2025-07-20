using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [Tooltip("The object that will be destroyed if overlapped")]
    public GameObject destroyedObject;

    void Start()
    {
        destroyedObject.SetActive(false);
    }
}