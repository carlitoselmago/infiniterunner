using UnityEngine;

public class SelfDestruct : MonoBehaviour, IResettable
{
    public void ResetState()
    {
        Destroy(gameObject);
    }
}