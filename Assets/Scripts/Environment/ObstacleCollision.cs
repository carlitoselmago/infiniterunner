﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    public GameObject thePlayer;
    public GameObject charModel;
    public AudioSource crashThud;
    public GameObject mainCam;
    public GameObject levelControl;
    /*
    void OnTriggerEnter(Collider objectName){
         //Output the Collider's GameObject's name
          Debug.Log("Entered collision with " + objectName.gameObject.name);
    }
    
    void OnTriggerEnter(Collider other)
    {
        //this.gameObject.GetComponent<BoxCollider>().enabled = false;
        //thePlayer.GetComponent<PlayerMove>().enabled = false;
        charModel.GetComponent<Animator>().Play("Stumble Backwards");
        crashThud.Play();
        mainCam.GetComponent<Animator>().enabled = true;
        levelControl.GetComponent<EndRunSequence>().enabled = true;
    }
    */
}