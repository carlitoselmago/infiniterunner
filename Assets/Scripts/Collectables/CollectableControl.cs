using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectableControl : MonoBehaviour
{
    public static int coinCount;
    public GameObject coinCountDisplay;
    public GameObject coinEndDisplay;

    //achievements vars
    public GameObject achievementUI;
    public GameObject achievementEndUItext;
    public static List<int> treballadordelmes_coins = new List<int> { 5, 50 };
    private int treballadordelmes_coins_index = 0;
    //private static int maxTimePlayed;
    //private float maxTimeResting = 60;

    // private bool HighScoreAchieved = false;
    // private bool SurvivalAchieved = false;
    //private bool RestfulAchieved = false;


    void Start()
    {
        coinCount = 0;
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount;
        coinEndDisplay.GetComponent<Text>().text = "" + coinCount;
        achievementUI.SetActive(false);
    }
    void Update()
    {
        coinCountDisplay.GetComponent<Text>().text = "" + coinCount;
        coinEndDisplay.GetComponent<Text>().text = "" + coinCount;

        if (treballadordelmes_coins_index < treballadordelmes_coins.Count)
        {
            if (coinCount >= treballadordelmes_coins[treballadordelmes_coins_index])
            {
                //highScoreSFX.Play();
                achievementEndUItext.GetComponent<Text>().text = "¡treballador del mes! \n" + treballadordelmes_coins[treballadordelmes_coins_index].ToString() + " monedes!";
                achievementUI.SetActive(true);
                treballadordelmes_coins_index += 1;
                StartCoroutine(hideachievement());
            }
        }
        /*
         if (coinCount >= maxScore && !HighScoreAchieved)
        {
            highScoreSFX.Play();
            highScorePopUp.SetActive(true);
            HighScoreAchieved = true;
            //StartCoroutine FadeOutFrame;

            //record new high score!
        }
        */
    }

    IEnumerator hideachievement()
    {
        yield return new WaitForSeconds(6);
        achievementUI.SetActive(false);
    }
}


