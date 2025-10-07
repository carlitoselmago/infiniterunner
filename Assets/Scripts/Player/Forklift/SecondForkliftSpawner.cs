using UnityEngine;

public class SecondForkliftSpawner : MonoBehaviour
{
    private float activationProb = 0.65f; // (0.0 to 1.0)

    // runs only if a first forklift was driven
    void OnEnable()
    {
        if (!CollectableControl.firstForklift)
            gameObject.SetActive(false);
        else
        {
            bool activateObject = Random.value < activationProb;
            gameObject.SetActive(activateObject);
        }
    }
}