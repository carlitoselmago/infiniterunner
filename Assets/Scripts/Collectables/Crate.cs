using UnityEngine;

public class Crate : MonoBehaviour, IResettable
{
    public GameObject resettableCrate;
    public GameObject explosionChild;
    public GameObject explosionCoins;
    private Explodable explodable;
    private Explodable explodableCoins;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        explodable = explosionChild.GetComponent<Explodable>();
        explodableCoins = explosionCoins.GetComponent<Explodable>();
    }

    private void OnEnable()
    {
        if (explosionChild.activeSelf)
        {
            explosionChild.SetActive(false);
            explosionCoins.SetActive(false);
        }
            
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        explosionChild.SetActive(true);
        explosionCoins.SetActive(true);
        explodable.enabled = true;
        explodableCoins.enabled = true;
        explodable.Explode();
        explodableCoins.Explode();
    }

    public void ResetState()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        if (!resettableCrate.gameObject.activeSelf)
            resettableCrate.gameObject.SetActive(true);
    }
}
