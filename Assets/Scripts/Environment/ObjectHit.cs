using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHit : MonoBehaviour
{
    public Animator animator;

    void OnTriggerEnter(Collider other)
    {
        animator.enabled = true;
    }
}