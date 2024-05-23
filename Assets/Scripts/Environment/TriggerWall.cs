using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TriggerWall : MonoBehaviour
{
    public GameObject triggeredObject;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Trigger"))
        {
            Debug.Log("TRIGGER");
            triggeredObject.SetActive(true);
        }
    }
}