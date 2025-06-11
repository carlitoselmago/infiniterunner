using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constrain : MonoBehaviour
{
    public PlayerMove player;
    public bool constrains = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Constrain active");
            player.SetConstrained(constrains);
        }
    }
}