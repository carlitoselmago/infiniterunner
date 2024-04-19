using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public GameObject[] section;
    public int zPos = 50;
    public bool creatingSection = false;

    public GameObject player;
    public int secNum;

    // Queue to store references to the instantiated sections
    private Queue<GameObject> createdSections = new Queue<GameObject>();

    void Start()
    {
        // Preload 3 sections at the start of the game
        for (int i = 0; i < 3; i++)
        {
            InstantiateInitialSection();
        }
    }

    void Update()
    {
        /*
        if (creatingSection == false)
        {
            creatingSection = true;
            StartCoroutine(GenerateSection());
        }
        */
        // Check if player's z-position is at or beyond the next trigger point and if we are not currently creating a section
        if (!creatingSection && player.transform.position.z >= zPos - 150)
        {
            creatingSection = true;
            GenerateSection();
        }
    }

    void GenerateSection()
    {
        secNum = Random.Range(0, 3);
        GameObject newSection = Instantiate(section[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
        zPos += 50;
        createdSections.Enqueue(newSection);  // Add the new section to the queue
        if (createdSections.Count > 4)
        {
            GameObject oldSection = createdSections.Dequeue();  // Remove the oldest section from the queue
            Destroy(oldSection);  // Destroy the oldest section object
        }
        // yield return new WaitForSeconds(3);
        creatingSection = false;
    }

      void InstantiateInitialSection()
    {
        GameObject newSection = Instantiate(section[Random.Range(0, section.Length)], new Vector3(0, 0, zPos), Quaternion.identity);
        zPos += 50;
        createdSections.Enqueue(newSection);
    }

}
