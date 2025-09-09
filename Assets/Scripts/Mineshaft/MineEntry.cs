using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


public class MineEntry : MonoBehaviour, IResettable
{
    public bool isExit = false; // define entry or exit point
    public GameObject levelControl;
    public GenerateSandstorm generateSandstorm;
    public Volume mineVolume;
    private GenerateLevel generateLevel;
    private bool triggered = false;

    void Start()
    {
        if (levelControl != null)
            generateLevel = levelControl.GetComponent<GenerateLevel>();
        else
            Debug.LogError("MineEntry: levelControl not assigned!");
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !triggered)
        {
            triggered = true;

            if (!isExit)
            {
                // Entering the mine
                MineData.isInTheMine = true;
                MineData.endlessFallDisabled = true;
                Debug.Log("Entering the mine");
                generateLevel.EnterMine();
                StartCoroutine(FadeVolume(0f, 1f, 3f));
                //generateSandstorm.StopTheSandstorm();
                //levelControl.GetComponent<GenerateSandstorm>().sandstormActive = false;
            }
            else
            {
                // Exiting the mine
                MineData.isInTheMine = false;
                Debug.Log("Exiting the mine");
                generateLevel.ExitMine();
                StartCoroutine(FadeVolume(1f, 0f, 1.5f));
                StartCoroutine(ReenableEndlessFall());
                //levelControl.GetComponent<GenerateSandstorm>().sandstormActive = true;
            }
        }
    }

    IEnumerator ReenableEndlessFall()
    {
        yield return new WaitForSeconds(5);
        MineData.endlessFallDisabled = false;
    }

    IEnumerator FadeVolume(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            mineVolume.weight = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mineVolume.weight = to;
    }

    public void ResetState()
    {
        triggered = false;
        mineVolume.weight = 0f;
    }

}