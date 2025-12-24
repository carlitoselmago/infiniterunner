using UnityEngine;

public class TriggerWall : MonoBehaviour, IResettable
{
    public GameObject triggeredObject;
    public bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            triggeredObject.SetActive(true);
            triggered = true;
        }
    }

    public void ResetState()
    {
        triggeredObject.SetActive(false);
        triggered = false;
    }
}