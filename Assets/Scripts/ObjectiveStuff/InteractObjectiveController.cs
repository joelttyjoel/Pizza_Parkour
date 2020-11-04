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
    public Transform firstBoxPosition;
    public Vector3 objectiveOffset;
    public GameObject[] allFakeColliders;

    private Transform objectiveContainer = null;
    private Transform closestObjective = null;
    private float closestObjectiveDistance = 99999999;
    private List<Transform> allAvailableObjectivesInScene = new List<Transform>();
    
    private Stack<Transform> allObjectivesOnHead = new Stack<Transform>();

    void Start()
    {
        GameObject[] allObjectives = GameObject.FindGameObjectsWithTag(objectiveTag);

        for (int i = 0; i < allObjectives.Length; i++)
        {
            allAvailableObjectivesInScene.Add(allObjectives[i].transform);
        }

        objectiveContainer = GameObject.Find("ObjectiveContainer").transform;
    }
    
    void Update()
    {
        UpdateFakeColliderOnHead();

        //BURST can be added here mm yes but not needed because probably not more than like 20 boxes
        //find closest objective
        foreach (Transform a in allAvailableObjectivesInScene)
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

        closestObjective.GetComponent<Rigidbody2D>().simulated = false;

        //remove from available list, doing this before so only need to compare transforms and not get instance id
        for (int i = 0; i < allAvailableObjectivesInScene.Count; i++)
        {
            if (allAvailableObjectivesInScene[i] == closestObjective)
            {
                allAvailableObjectivesInScene.RemoveAt(i);
                break;
            }
        }
        //reset closest distance so will check for ntext closest
        closestObjectiveDistance = 9999999f;
        //change transform and move
        closestObjective.SetParent(firstBoxPosition);
        closestObjective.position = firstBoxPosition.position + (objectiveOffset * allObjectivesOnHead.Count);
        //add to stack on head
        allObjectivesOnHead.Push(closestObjective);
    }

    public void DropTopObjective()
    {
        if (allObjectivesOnHead.Count <= 0) return;

        Debug.Log("drop");

        //remove from all on head
        Transform objectToDrop = allObjectivesOnHead.Pop();

        objectToDrop.GetComponent<Rigidbody2D>().simulated = true;
        
        //now that removed from head, remove top of fake ones, mm yes
        UpdateFakeColliderOnHead();

        //add back to list of all avaiable for pickup
        allAvailableObjectivesInScene.Add(objectToDrop);

        //change transform
        objectToDrop.SetParent(objectiveContainer);
        //find where to place, hardest part hmmm
        
    }

    public void UpdateFakeColliderOnHead()
    {
        for(int i = 0; i < allFakeColliders.Length; i++)
        {
            if (i < allObjectivesOnHead.Count) allFakeColliders[i].SetActive(true);
            else allFakeColliders[i].SetActive(false);
        }
    }
}
