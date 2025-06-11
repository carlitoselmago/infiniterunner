using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constrain : MonoBehaviour
{
    public PlayerMove player;

    // Set which positions are constrained in the Inspector
    public bool constrainLeft = false;
    public bool constrainCenter = false;
    public bool constrainRight = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.SetConstrainedPositions(constrainLeft, constrainCenter, constrainRight);
        }
    }
}