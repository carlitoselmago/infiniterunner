using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float activationProbability = 0.5f; // (0.0 to 1.0)

    void Start()
    {
        bool activateObject = Random.value < activationProbability;
        gameObject.SetActive(activateObject);
    }
}
