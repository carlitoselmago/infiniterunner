using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyingCountdown : MonoBehaviour
{
    public static float countdown = 10;
    public GameObject countdownDisplay;
    public Text countdownText;
    public GameObject levelControl;

    void Start()
    {
        countdownDisplay.SetActive(true);
        countdown = 20;
    }

    void Update()
    {
        if (countdown > 0)
        {
            float seconds = Mathf.FloorToInt(countdown % 60);
            countdown -= Time.deltaTime;
            countdownText.GetComponent<Text>().text = seconds + "s";
        }
        else
        {
            StartCoroutine(wait());
            countdown = 0;
        }
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(2);
        countdownDisplay.SetActive(false);
        levelControl.GetComponent<FlyingCountdown>().enabled = false;
    }
}
