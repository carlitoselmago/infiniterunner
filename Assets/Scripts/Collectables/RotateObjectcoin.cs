using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectcoin : MonoBehaviour
{
    public float rotateSpeed = 1;
    private float startingPos = 0;

    private void Start()
    {
       // startingPos = Random.Range(0, 360); // defines initial rotation position
        //transform.Rotate(0, 0, startingPos, Space.Self);
        startingPos = transform.position.z*8f;
        transform.Rotate(0,startingPos, 0, Space.Self);
    }
    void Update()
    {
        transform.Rotate( 0,rotateSpeed, 0, Space.Self);
    }
}