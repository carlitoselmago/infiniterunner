using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtMask : MonoBehaviour
{
    public GameObject hurtMask;

    public IEnumerator Mask()
    {
        hurtMask.SetActive(true);
        yield return new WaitForSeconds(0.55f);
        hurtMask.SetActive(false);
    }
}