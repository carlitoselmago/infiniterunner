using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ChimneyCulling : MonoBehaviour
{
    public Transform cam;               // Assign the player camera (defaults to Camera.main)
    public float maxDistance = 300f;     // Only emit smoke when chimney is this close in front

    private ParticleSystem ps;
    private bool isCulled;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
    }

    void Update()
    {
        if (cam == null) return;

        Vector3 toChimney = transform.position - cam.position;
        float distance = toChimney.magnitude;

        // Dot product: > 0 means in front, < 0 means behind
        bool isInFront = Vector3.Dot(cam.forward, toChimney.normalized) > 0f;

        // Cull if behind or too far ahead
        bool shouldCull = !isInFront || distance > maxDistance;

        if (shouldCull && !isCulled)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            //Debug.Log("Stopped Smoke Particles");
            isCulled = true;
        }
        else if (!shouldCull && isCulled)
        {
            ps.Play();
            isCulled = false;
        }
    }
}
