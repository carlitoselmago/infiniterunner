using System.Collections;
using UnityEngine;

public class GenerateTunnel : MonoBehaviour
{
    public GameObject walkTunnel;
    public GameObject minecartTunnel;

    private bool triggered = false; // ensures it only runs once

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            float activatedSection = Random.Range(0f, 1f);

            if (activatedSection >= 0.5f)
            {
                walkTunnel.SetActive(true);
                minecartTunnel.SetActive(false);
                Debug.Log("Walk tunnel activated");
            }
            else
            {
                minecartTunnel.SetActive(true);
                walkTunnel.SetActive(false);
                Debug.Log("Minecart tunnel activated");
            }
        }
    }

}
