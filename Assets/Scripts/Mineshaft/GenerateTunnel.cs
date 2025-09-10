using UnityEngine;

public class GenerateTunnel : MonoBehaviour, IResettable
{
    public GameObject object1;
    public GameObject object2;

    private bool triggered = false; // ensures it only runs once

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;

            float activatedSection = Random.Range(0f, 1f);

            if (activatedSection >= 0.5f)
            {
                object1.SetActive(true);
                object2.SetActive(false);
                Debug.Log("Object 1 activated");
            }
            else
            {
                object2.SetActive(true);
                object1.SetActive(false);
                Debug.Log("Object 2 activated");
            }
        }
    }

    public void ResetState()
    {
        triggered = false;
    }

}
