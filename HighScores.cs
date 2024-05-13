using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class HighScores : MonoBehaviour
{
    public GameObject highScorePopUp;
    public GameObject survivalPopUp;
    public GameObject restfulPopUp;

    private static int maxScore;
    private static int maxTimePlayed;
    private float maxTimeResting = 60;

    private bool HighScoreAchieved = false;
    private bool SurvivalAchieved = false;
    private bool RestfulAchieved = false;

    public AudioSource highScoreSFX;
    public AudioSource survivalSFX;
    public AudioSource restSFX;

    public static int coinCount;



    void Start()
    {

    }

    void Update()
    {

        //get coinCount from CollectableControl
        //start recording time
//start counting time after any coin has been picked up

        if (coinCount >= maxScore && !HighScoreAchieved)
        {
            highScoreSFX.Play();
            highScorePopUp.SetActive(true);
            HighScoreAchieved = true;
            //StartCoroutine FadeOutFrame;

            //record new high score!
        }
    }
}
