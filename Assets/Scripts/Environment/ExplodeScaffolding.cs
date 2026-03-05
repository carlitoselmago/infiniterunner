using UnityEngine;

public class ExplodeScaffolding : MonoBehaviour, IResettable
{
    public GameObject scaffolding;
    private Explodable explodable;
    public bool triggered = false;

    private void OnEnable()
    {
        explodable = scaffolding.GetComponent<Explodable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && ! triggered && explodable != null)
        {
            explodable = scaffolding.GetComponent<Explodable>();
            explodable.enabled = true;
            explodable.Explode();
            //Debug.Log("Explosion");
            triggered = true;
        }
    }

    public void ResetState()
    {
        triggered = false;
    }
}