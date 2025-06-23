using UnityEngine;

public class TriggerWall : MonoBehaviour
{
    public GameObject triggeredObject;
    //public GameObject setInactiveObject;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("TRIGGER");
            triggeredObject.SetActive(true);
            //setInactiveObject.SetActive(false);
        }
    }
}