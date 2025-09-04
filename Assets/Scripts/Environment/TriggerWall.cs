using UnityEngine;

public class TriggerWall : MonoBehaviour
{
    public GameObject triggeredObject;
    public bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            //Debug.Log("TRIGGER");
            triggeredObject.SetActive(true);
            triggered = true;
        }
    }
}