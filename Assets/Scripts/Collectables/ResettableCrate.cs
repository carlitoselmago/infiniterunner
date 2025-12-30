using UnityEngine;

public class ResettableCrate : MonoBehaviour, IResettable
{
    public void ResetState()
    {
        //gameObject.SetActive(true);

        foreach (Transform child in transform)
            child.gameObject.SetActive(true);
    }
}