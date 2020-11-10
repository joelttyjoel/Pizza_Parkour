using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [Header("Drop stuff")]
    public Transform leftDropPosition;
    public Transform rightDropPosition;
    public LayerMask layerMaskToIgnore;
    public Vector3 centerRaycastsOffset;
    public Vector3[] raycastsLeftCheck;
    public Vector3[] raycastsRightCheck;
    private float powerThrow;
    public float powerPerSecond;
    public float maxPower;
    public float powerThrowMultiplier;
    public Text indicatorPowerThrow;

    private Transform objectiveContainer = null;
    private Transform closestObjective = null;
    private float closestObjectiveDistance = 99999999;
    private List<Transform> allAvailableObjectivesInScene = new List<Transform>();
    private bool canDropLeft = false;
    private bool canDropRight = false;

    //should be in movement script
    private bool isGoingLeft = false;

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

        if (Input.GetKeyDown(KeyCode.D)) isGoingLeft = false;
        else if (Input.GetKey(KeyCode.A)) isGoingLeft = true;

        //CheckSide(raycastsLeftCheck, canDropLeft);
        //CheckSide(raycastsRightCheck, canDropRight);

        //------EXTRA CHECKS BECAUE CAN
        if (allObjectivesOnHead.Count <= 0) return;
        //-----------------------------

        //if click drop
        //button down, start charge, on release, drop
        if (Input.GetKeyDown(KeyCode.F))
        {
            indicatorPowerThrow.gameObject.SetActive(true);
        }
        if (Input.GetKey(KeyCode.F))
        {
            powerThrow += powerPerSecond * Time.deltaTime;
            if (powerThrow >= maxPower) powerThrow = maxPower;
            indicatorPowerThrow.text = ((int)powerThrow).ToString();
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            DropTopObjective();
            powerThrow = 0f;
            indicatorPowerThrow.gameObject.SetActive(false);
        }
    }

    private void PickUpClosestObjective()
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

    private void DropTopObjective()
    {
        if (allObjectivesOnHead.Count <= 0) return;

        //do checks for sides
        CheckSide(raycastsLeftCheck, ref canDropLeft);
        CheckSide(raycastsRightCheck, ref canDropRight);

        //if both checks fail, cant drop
        if (!canDropLeft && !canDropRight) return;

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
        //right with fail on left
        if(isGoingLeft && canDropLeft)
        {
            objectToDrop.transform.position = leftDropPosition.position;
            //apply force in direction of drop
            objectToDrop.GetComponent<Rigidbody2D>().AddForce(Vector2.left * powerThrow * powerThrowMultiplier);
        }
        else if(canDropRight)
        {
            objectToDrop.transform.position = rightDropPosition.position;
            objectToDrop.GetComponent<Rigidbody2D>().AddForce(Vector2.right * powerThrow * powerThrowMultiplier);
        }
        //left with fail on right
        if (!isGoingLeft && canDropRight)
        {
            objectToDrop.transform.position = rightDropPosition.position;
            objectToDrop.GetComponent<Rigidbody2D>().AddForce(Vector2.right * powerThrow * powerThrowMultiplier);
        }
        else if (canDropLeft)
        {
            objectToDrop.transform.position = leftDropPosition.position;
            objectToDrop.GetComponent<Rigidbody2D>().AddForce(Vector2.left * powerThrow * powerThrowMultiplier);
        }
        //else, just stays on head
    }

    private void UpdateFakeColliderOnHead()
    {
        for(int i = 0; i < allFakeColliders.Length; i++)
        {
            if (i < allObjectivesOnHead.Count) allFakeColliders[i].SetActive(true);
            else allFakeColliders[i].SetActive(false);
        }
    }

    private void CheckSide(Vector3[] sideToCheck, ref bool sideBool)
    {
        Vector3 origin = transform.position + centerRaycastsOffset;
        sideBool = true;
        for(int i = 0; i < sideToCheck.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, sideToCheck[i], sideToCheck[i].magnitude, ~layerMaskToIgnore);
            
            if (hit.collider != null)
            {
                //Debug.Log(hit.collider.gameObject.name);
                sideBool = false;
            }

            Debug.DrawLine(origin, origin + sideToCheck[i], (sideBool ? Color.green : Color.red));
        }
        //Debug.Log(sideBool);
    }

    //private void OnDrawGizmos()
    //{
    //    Vector3 origin = transform.position + centerRaycastsOffset;
        
    //    for (int i = 0; i < raycastsLeftCheck.Length; i++)
    //    {
    //        Gizmos.DrawLine(origin, origin + raycastsLeftCheck[i]);
    //    }
    //    for (int i = 0; i < raycastsRightCheck.Length; i++)
    //    {
    //        Gizmos.DrawLine(origin, origin + raycastsRightCheck[i]);
    //    }
    //}
}
