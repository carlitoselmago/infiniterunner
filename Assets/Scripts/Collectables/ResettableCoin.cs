using UnityEngine;

public class ResettableCoin : MonoBehaviour, IResettable
{
    public void ResetState()
    {
        gameObject.SetActive(true);
    }
}