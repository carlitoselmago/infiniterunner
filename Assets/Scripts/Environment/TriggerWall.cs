using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TriggerWall : MonoBehaviour
{
    public GameObject triggeredObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Trigger"))
        {
            triggeredObject.SetActive(true);
        }
    }
}