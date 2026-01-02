using UnityEngine;

public class ResettableCrate : MonoBehaviour, IResettable
{
    public void ResetState()
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(true);
    }
}