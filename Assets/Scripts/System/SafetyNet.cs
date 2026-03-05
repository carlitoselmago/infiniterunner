using UnityEngine;

public class SafetyNet : MonoBehaviour, IResettable
{

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldDisable(other.gameObject))
        {
            other.gameObject.SetActive(false);
            Debug.Log("Falling object caught in the safety net: " + other.name);
        }
    }

    private bool ShouldDisable(GameObject obj)
    {
        // Ignore self
        if (obj == gameObject)
            return false;
        // Ignore player
        if (obj.CompareTag("Player"))
            return false;
        // Ignore minewalls
        if (obj.CompareTag("minewall"))
            return false;
        // Ignore deep falling triggers
        if (obj.CompareTag("fall"))
            return false;
        
        return true;
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
    }
}