/*using UnityEngine;

public class ResettableCollectable : MonoBehaviour, IResettableChild
{
    private Vector3 initialPos;
    private Quaternion initialRot;
    private bool initialActive;

    void Awake()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
        initialActive = gameObject.activeSelf;
    }

    public void ResetState()
    {
        transform.localPosition = initialPos;
        transform.localRotation = initialRot;
        gameObject.SetActive(initialActive);
    }
}*/