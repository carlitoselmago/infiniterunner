using UnityEngine;

public class ResettableCoin : MonoBehaviour, IResettable
{
    public void ResetState()
    {
        gameObject.SetActive(true);
        foreach (Transform child in transform)
            if (!child.gameObject.activeSelf)
                child.gameObject.SetActive(true);
    }
}