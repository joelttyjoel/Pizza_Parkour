using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public string objectiveTag;
    public List<Transform> allObjectives;

    // Start is called before the first frame update
    void Awake()
    {
        GameObject[] allObjectivesInScene = GameObject.FindGameObjectsWithTag(objectiveTag);

        for(int i = 0; i < allObjectivesInScene.Length; i++)
        {
            allObjectives.Add(allObjectivesInScene[i].transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
