using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class HighestScore : MonoBehaviour, IResettable
{
    public GameObject cogs;
    public float maxRadius = 2f;        // final radius of the explosion
    public float expansionTime = 0.5f;  // how fast it grows
    public float explosionForce = 200f; // force applied to objects
    public float explosionUpward = 0.7f;// adds a vertical lift

    private SphereCollider sphereCol;

    // Store original positions & rotations of all cogs
    private Dictionary<Rigidbody, (Vector3 pos, Quaternion rot)> cogStartStates;

    void Awake()
    {
        sphereCol = GetComponent<SphereCollider>();
        sphereCol.isTrigger = true;
        sphereCol.radius = 0f;

        // Cache initial state of cogs and all child rigidbodies
        cogStartStates = new Dictionary<Rigidbody, (Vector3, Quaternion)>();
        foreach (var rb in cogs.GetComponentsInChildren<Rigidbody>())
            cogStartStates[rb] = (rb.transform.position, rb.transform.rotation);

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

            rb.transform.position = pos;
            rb.transform.rotation = rot;

            // Also reset velocities if they’re simulated
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        cogs.SetActive(true);
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

            rb.transform.position = pos;
            rb.transform.rotation = rot;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}


/*using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class HighestScore : MonoBehaviour, IResettable
{
    public GameObject cogs;
    public float maxRadius = 2f;        // final radius of the explosion
    public float expansionTime = 0.5f;  // how fast it grows
    public float explosionForce = 200f; // force applied to objects
    public float explosionUpward = 0.7f;// adds a vertical lift

    private SphereCollider sphereCol;

    void Awake()
    {
        sphereCol = GetComponent<SphereCollider>();
        sphereCol.isTrigger = true; // explosion doesn’t physically collide
        sphereCol.radius = 0f;      // start at 0
        gameObject.SetActive(false);
        cogs.SetActive(false); // stays off until triggered
    }

    void OnEnable()
    {
        // restart every time this object is reused
        sphereCol.radius = 0f;
        StartCoroutine(Expand());
        cogs.SetActive(true);
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
            rb.AddExplosionForce(explosionForce, transform.position, maxRadius, explosionUpward, ForceMode.Impulse);
    }

    public void ResetState()
    {
        gameObject.SetActive(false);
        cogs.SetActive(false);
    }
}
*/