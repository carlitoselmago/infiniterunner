using UnityEngine;

public class Crate : MonoBehaviour, IResettable
{
    public GameObject resettableCrate;
    public GameObject explosionChild;
    private Explodable explodable;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        explodable = explosionChild.GetComponent<Explodable>();
    }

    private void OnEnable()
    {
        if (explosionChild.activeSelf)
            explosionChild.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        explosionChild.SetActive(true);
        explodable.enabled = true;
        explodable.Explode();

    }

    public void ResetState()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        if (!resettableCrate.gameObject.activeSelf)
            resettableCrate.gameObject.SetActive(true);
    }
}
