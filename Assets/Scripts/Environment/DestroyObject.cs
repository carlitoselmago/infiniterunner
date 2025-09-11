using UnityEngine;
using System.Collections;

public class DestroyObject : MonoBehaviour
{
    [Tooltip("The object that will be destroyed if overlapped")]
    public GameObject destroyedObject;

    void OnEnable()
    {
        StartCoroutine(CheckAndDestroy());
    }

    IEnumerator CheckAndDestroy()
    {
        yield return new WaitForSeconds(0.05f); // Let things settle
        destroyedObject.SetActive(false);
    }
}