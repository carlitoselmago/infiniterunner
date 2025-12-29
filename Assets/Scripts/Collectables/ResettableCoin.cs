using UnityEngine;

public class ResettableCoin : MonoBehaviour, IResettable
{
    private void OnEnable()
    {
        ResetCoins();
    }

    private void ResetCoins()
    {
        foreach (Transform child in transform)
                child.gameObject.SetActive(true);
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
        ResetCoins();
    }
}