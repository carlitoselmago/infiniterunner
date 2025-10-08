using UnityEngine;

public class RideCart : MonoBehaviour
{
    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
