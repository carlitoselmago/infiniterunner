using UnityEngine;

public class HitLogic : MonoBehaviour
{
    public enum HitboxType { Normal, Jump, Crouch, None }

    [Header("Hitboxes")]
    public Collider normalHitbox;   // assign trigger collider (normal)
    public Collider jumpHitbox;     // assign trigger collider (jump)
    public Collider crouchHitbox;   // assign trigger collider (crouch)

    private PlayerMove playerMove;

    void Awake()
    {
        playerMove = GetComponentInParent<PlayerMove>();
        // Ensure we have a kinematic Rigidbody so triggers reliably fire:
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Start()
    {
        // Auto-find trigger colliders if not assigned
        Collider[] cols = GetComponents<Collider>();
        foreach (var c in cols)
        {
            if (c.isTrigger)
            {
                if (normalHitbox == null) normalHitbox = c;
                else if (jumpHitbox == null) jumpHitbox = c;
                else if (crouchHitbox == null) crouchHitbox = c;
            }
        }

        EnableHitbox(HitboxType.Normal); // default
    }

    /// <summary>
    /// Enables exactly one hitbox (Normal, Jump, Crouch), or disables all (None).
    /// </summary>
    public void EnableHitbox(HitboxType type)
    {
        if (normalHitbox != null) normalHitbox.enabled = (type == HitboxType.Normal);
        if (jumpHitbox != null) jumpHitbox.enabled = (type == HitboxType.Jump);
        if (crouchHitbox != null) crouchHitbox.enabled = (type == HitboxType.Crouch);

        if (type == HitboxType.None)
        {
            if (normalHitbox != null) normalHitbox.enabled = false;
            if (jumpHitbox != null) jumpHitbox.enabled = false;
            if (crouchHitbox != null) crouchHitbox.enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (playerMove == null) return;

        // ignore collisions caused by the player's own physics rigidbody (safety)
        if (other.attachedRigidbody == playerMove.playerBody) return;

        // forward the trigger to the parent for processing
        playerMove.ProcessTrigger(other);
    }
}