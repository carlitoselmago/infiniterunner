using UnityEngine;
using System.Collections.Generic;

public class Explodable : MonoBehaviour, IResettable
{
    [Header("Explosion Settings")]
    public float explosionForce = 5f;
    public float explosionRadius = 3f;
    public float upwardsModifier = 0.5f;

    private List<Transform> parts = new List<Transform>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Quaternion> originalRotations = new List<Quaternion>();
    private List<Rigidbody> partRigidbodies = new List<Rigidbody>();

    private void Awake()
    {
        // Recursively collect all children
        //CollectParts(transform);
    }

    private void CollectParts(Transform parent)
    {
        foreach (Transform child in parent)
        {
            parts.Add(child);
            originalPositions.Add(child.localPosition);
            originalRotations.Add(child.localRotation);

            // Ensure Rigidbody exists
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb == null)
                rb = child.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            partRigidbodies.Add(rb);

            // Ensure Collider exists
            if (child.GetComponent<Collider>() == null)
                child.gameObject.AddComponent<BoxCollider>();

            // Recurse into children
            if (child.childCount > 0)
                CollectParts(child);
        }
    }

    private void OnEnable()
    {
        CollectParts(transform); // experimental
        Explode();
    }

    public void Explode()
    {
        for (int i = 0; i < parts.Count; i++)
        {
            Rigidbody rb = partRigidbodies[i];
            rb.isKinematic = false;
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
        }
    }

    public void ResetState()
    {
        for (int i = 0; i < parts.Count; i++)
        {
            Transform part = parts[i];
            part.localPosition = originalPositions[i];
            part.localRotation = originalRotations[i];

            Rigidbody rb = partRigidbodies[i];
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private void OnDisable()
    {
        ResetState();
    }
}
