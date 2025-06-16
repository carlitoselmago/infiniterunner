using UnityEngine;

public class TriggerWall : MonoBehaviour
{
    public GameObject triggeredObject;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("TRIGGER");
            triggeredObject.SetActive(true);
        }
    }
}