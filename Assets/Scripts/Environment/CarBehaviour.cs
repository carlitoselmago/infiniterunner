using UnityEngine;
using System.Collections;

public class CarBehaviour : MonoBehaviour
{
    private float interval;
    public Animator animator;

    void Start()
    {
        animator.enabled = false;
        interval = Random.Range(0f, 8f);
        StartCoroutine(Activate());
    }

    IEnumerator Activate()
    {
        yield return new WaitForSeconds(interval);
        animator.enabled = true;
    }
}