using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Animator))]

public class HighestScore : MonoBehaviour, IResettable
{
    public GameObject cogs;
    public float maxRadius = 2f;        // final radius of the explosion
    public float expansionTime = 0.5f;  // how fast it grows
    public float explosionForce = 200f; // force applied to objects
    public float explosionUpward = 0.7f;// adds a vertical lift

    private SphereCollider sphereCol;
    private Animator animator;

    // Store original positions & rotations of all cogs
    private Dictionary<Rigidbody, (Vector3 pos, Quaternion rot)> cogStartStates;

    void Awake()
    {
        sphereCol = GetComponent<SphereCollider>();
        sphereCol.isTrigger = true;
        sphereCol.radius = 0f;

        animator = GetComponent<Animator>();

        // Cache initial state of cogs and all child rigidbodies
        cogStartStates = new Dictionary<Rigidbody, (Vector3, Quaternion)>();
        foreach (var rb in cogs.GetComponentsInChildren<Rigidbody>())
            cogStartStates[rb] = (rb.transform.localPosition, rb.transform.localRotation);

        gameObject.SetActive(false);
        cogs.SetActive(false);
    }

    void OnEnable()
    {
        // Reset collider expansion
        sphereCol.radius = 0f;
        StartCoroutine(Expand());

        // Reset cog positions/rotations
        foreach (var kvp in cogStartStates)
        {
            Rigidbody rb = kvp.Key;
            (Vector3 pos, Quaternion rot) = kvp.Value;

            rb.transform.localPosition = pos;
            rb.transform.localRotation = rot;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reset animator to its first frame
        animator.Rebind();
        animator.Update(0f);

        cogs.SetActive(true);
    }

    void OnDisable()
    {
        ResetState();
    }

    IEnumerator Expand()
    {
        float elapsed = 0f;
        while (elapsed < expansionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expansionTime;
            sphereCol.radius = Mathf.Lerp(0f, maxRadius, t);
            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.AddExplosionForce(explosionForce, transform.position, maxRadius, explosionUpward, ForceMode.Impulse);
        }
    }

    public void ResetState()
    {
        // Disable explosion + cogs
        gameObject.SetActive(false);
        cogs.SetActive(false);

        // Reset transforms of all cogs so they’re ready for next run
        foreach (var kvp in cogStartStates)
        {
            Rigidbody rb = kvp.Key;
            (Vector3 pos, Quaternion rot) = kvp.Value;

            rb.transform.localPosition = pos;
            rb.transform.localRotation = rot;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        animator.Rebind();
        animator.Update(0f);
    }
}

