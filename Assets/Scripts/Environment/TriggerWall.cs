using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TriggerWall : MonoBehaviour
{
    public GameObject panopticPoster;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("fade trigger"))
        {
            panopticPoster.SetActive(true);
        }
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(10);
        panopticPoster.SetActive(false);
    }
}