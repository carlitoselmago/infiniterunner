using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float activationProbability = 0.5f; // (0.0 to 1.0)

    void OnEnable()
    {
        bool activateObject = Random.value < activationProbability;
        gameObject.SetActive(activateObject);
    }
}