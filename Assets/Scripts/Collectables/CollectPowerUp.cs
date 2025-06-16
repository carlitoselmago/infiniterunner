/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectPowerUp : MonoBehaviour
{
    public AudioSource powerUpFX;
    public bool nowCanFly = false;
    public GameObject levelControl;

    void OnTriggerEnter(Collider other)
    {
        if (nowCanFly == false)
        {
            nowCanFly = true;
            PlayerMove.canFly = true;
            levelControl.GetComponent<FlyingCountdown>().enabled = true;
        }
        this.gameObject.SetActive(false);
    }
}*/