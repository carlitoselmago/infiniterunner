﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public int stepamount=100;
    public GameObject templatesparent;
    //public GameObject doubleSection;
    private GameObject[] section;
    public AudioSource mainTheme;
    
    private int zPos;
    public int generatedSections = 0;
    public bool creatingSection = false;

    public GameObject MAP;
    public int secNum;

    // Queue to store references to the instantiated sections
    private Queue<GameObject> createdSections = new Queue<GameObject>();

   void Start()
{
    zPos =200;// stepamount*2;

    // Initialize the section array with the number of children in templatesparent
    section = new GameObject[templatesparent.transform.childCount];

    // Loop through each child, set its position to 0,0,0, and add it to the section array
    for (int i = 0; i < templatesparent.transform.childCount; i++)
    {
        GameObject child = templatesparent.transform.GetChild(i).gameObject;
        child.transform.localPosition = Vector3.zero;  // Reset the position of each child
        section[i] = child;
    }

    // Preload 3 sections at the start of the game
    for (int i = 0; i < 4; i++)
    {
        InstantiateInitialSection();
    }
}

  void Update()
    {
        // Check if the map has moved enough to require a new section
        if (MAP.transform.position.z < -zPos + (stepamount*4) && !creatingSection)
        {
            creatingSection = true;
            GenerateSection();
            generatedSections ++;
        }
    }

    void GenerateSection()
    {
        secNum = Random.Range(0, section.Length);
        GameObject newSection = Instantiate(section[secNum], new Vector3(0, 0, zPos), Quaternion.identity);
        newSection.transform.SetParent(MAP.transform, false);
        zPos += stepamount;
        createdSections.Enqueue(newSection);  // Add the new section to the queue
        if (createdSections.Count > 6)
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
         // Set the parent of the instantiated child to MAP
        newSection.transform.SetParent(MAP.transform, false);
        zPos += stepamount;
        createdSections.Enqueue(newSection);
    }
}