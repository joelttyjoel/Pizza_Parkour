using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractObjectiveController : MonoBehaviour
{
    [Header("References")]
    public string objectiveTag;
    public GameObject pickUpHint;
    [Header("Settings")]
    public float pickupRange;
    public Vector3 firstBoxPosition;
    public Vector3 boxOffset;

    private Transform closestObjective = null;
    private float closestObjectiveDistance = 99999999;
    private List<Transform> allObjectivesInScene = new List<Transform>();
    
    void Start()
    {
        GameObject[] allObjectives = GameObject.FindGameObjectsWithTag(objectiveTag);

        for (int i = 0; i < allObjectives.Length; i++)
        {
            allObjectivesInScene.Add(allObjectives[i].transform);
        }
    }
    
    void Update()
    {
        //BURST can be added here mm yes but not needed because probably not more than like 20 boxes
        //find closest objective
        foreach(Transform a in allObjectivesInScene)
        {
            float currentDistance = Vector3.Distance(transform.position, a.position);
            if (currentDistance < closestObjectiveDistance) closestObjective = a;
        }
        closestObjectiveDistance = Vector3.Distance(transform.position, closestObjective.position);

        //if withing range
        if(closestObjectiveDistance < pickupRange)
        {
            pickUpHint.SetActive(true);
            if (Input.GetKeyDown(KeyCode.R)) PickUpClosestObjective();
        }
        else
        {
            pickUpHint.SetActive(false);
        }

        //if click drop
        if (Input.GetKeyDown(KeyCode.F)) DropTopObjective();
    }

    public void PickUpClosestObjective()
    {
        Debug.Log("pick up");
    }

    public void DropTopObjective()
    {
        Debug.Log("drop");
    }
}
