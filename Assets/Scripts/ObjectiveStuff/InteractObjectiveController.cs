using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MovementScript))]
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
    public GameObject[] allFakeCollidersTriggers;
    public LayerMask layerMaskToIgnoreForHeadChecks;
    [Header("Drop stuff")]
    public Transform leftDropPosition;
    public Transform rightDropPosition;
    public LayerMask layerMaskToOnlyHitForSideChecks;
    public LayerMask layerMaskToIgnoreForSideChecks;
    public Vector3 centerRaycastsOffset;
    public Vector3[] raycastsLeftCheck;
    public Vector3[] raycastsRightCheck;
    public float powerThrow;

    private Transform objectiveContainer = null;
    private Transform closestObjective = null;
    private float closestObjectiveDistance = 99999999;
    private List<Transform> allAvailableObjectivesInScene = new List<Transform>();
    private bool canDropLeft = false;
    private bool canDropRight = false;

    PlayerSounds playerSounds;

    private Stack<Transform> allObjectivesOnHead = new Stack<Transform>();

    void Start()
    {

        GameObject[] allObjectives = GameObject.FindGameObjectsWithTag(objectiveTag);

        for (int i = 0; i < allObjectives.Length; i++)
        {
            allAvailableObjectivesInScene.Add(allObjectives[i].transform);
        }

        objectiveContainer = GameObject.Find("ObjectiveContainer").transform;

        //put all objectives on head
        while (allAvailableObjectivesInScene.Count > 0)
        {
            FindClosestObjective();
            PickUpClosestObjective();
        }

        playerSounds = GetComponent<PlayerSounds>();    //Keep this order, makes sure to not play pickup-sound when all are autopicked up..
        if (playerSounds == null)
            Debug.LogWarning("No PlayerSounds script attached to player object, no player-sounds will play.", this);
    }

    void Update()
    {
        UpdateFakeColliderOnHead();

        //BURST can be added here mm yes but not needed because probably not more than like 20 boxes
        FindClosestObjective();

        //if withing range and can pickup
        if (closestObjectiveDistance < pickupRange && PickUpHeadCheck())
        {
            pickUpHint.SetActive(true);
            pickUpHint.GetComponent<Image>().sprite = closestObjective.GetComponent<SpriteRenderer>().sprite;
            if (Input.GetButtonDown("Pickup")) PickUpClosestObjective();
        }
        else
        {
            pickUpHint.SetActive(false);
        }



        //CheckSide(raycastsLeftCheck, canDropLeft);
        //CheckSide(raycastsRightCheck, canDropRight);

        //------EXTRA CHECKS BECAUE CAN
        if (allObjectivesOnHead.Count <= 0)
        {
            GetComponent<MovementScript>().SetHolding(false);
            return;
        }
        else
            GetComponent<MovementScript>().SetHolding(true);

        //-----------------------------
        //drop
        if (Input.GetButtonDown("Fire1"))
        {
            DropTopObjective(false);
        }
        //throw
        if (Input.GetButtonDown("Fire2"))
        {
            DropTopObjective(true);
        }
    }

    public int GetObjectivesOnHeadCount()
    {
        return allObjectivesOnHead.Count;
    }

    public void RemoveObjectiveFromWorld(Transform objectiveToRemove)
    {
        for (int i = 0; i < allAvailableObjectivesInScene.Count; i++)
        {
            if (allAvailableObjectivesInScene[i] == objectiveToRemove)
            {
                Debug.Log(allAvailableObjectivesInScene[i].name);
                Debug.Log(objectiveToRemove.name);
                allAvailableObjectivesInScene.RemoveAt(i);
                Destroy(objectiveToRemove.gameObject);
                closestObjective = null;
                closestObjectiveDistance = 999999f;
                FindClosestObjective();
                return;
            }
        }
    }

    public void DisableObjectInWorld(Transform objectiveToRemove)
    {
        for (int i = 0; i < allAvailableObjectivesInScene.Count; i++)
        {
            if (allAvailableObjectivesInScene[i] == objectiveToRemove)
            {
                allAvailableObjectivesInScene.RemoveAt(i);
                closestObjective = null;
                closestObjectiveDistance = 999999f;
                FindClosestObjective();
                return;
            }
        }
    }

    //not needed because wont be any objectives left when level completed
    //public void StopAllDecay()
    //{
    //    for (int i = 0; i < allAvailableObjectivesInScene.Count; i++)
    //    {
    //        allAvailableObjectivesInScene[i].GetComponent<ObjectiveSelfController>().StopDecaying();
    //    }
    //}

    private void FindClosestObjective()
    {
        if (allAvailableObjectivesInScene.Count == 0)
        {
            closestObjective = null;
            closestObjectiveDistance = 999999f;
            return;
        }

        foreach (Transform a in allAvailableObjectivesInScene)
        {
            float currentDistance = Vector3.Distance(transform.position, a.position);
            if (currentDistance < closestObjectiveDistance) closestObjective = a;
        }
        closestObjectiveDistance = Vector3.Distance(transform.position, closestObjective.position);
    }

    private bool PickUpHeadCheck()
    {
        bool canPickUp = false;

        //check if next box space to be picked up is not overlapping with anything
        Collider2D relevantBoxOnHead = allFakeCollidersTriggers[allObjectivesOnHead.Count].GetComponent<BoxCollider2D>();
        Collider2D[] colliders = new Collider2D[10];
        //filter out objectives
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(~layerMaskToIgnoreForHeadChecks);
        int colliderCount = relevantBoxOnHead.OverlapCollider(contactFilter, colliders);

        //Debug.Log(colliderCount);

        if (colliderCount < 1) canPickUp = true;

        return canPickUp;
    }

    private void PickUpClosestObjective()
    {
        //stop decaying
        closestObjective.GetComponent<ObjectiveSelfController>().StopDecaying();

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

        playerSounds?.PlayPickup();
    }

    public void DropTopObjective(bool shouldThrow)
    {
        if (allObjectivesOnHead.Count <= 0) return;

        //do checks for sides
        CheckSide(raycastsLeftCheck, ref canDropLeft);
        CheckSide(raycastsRightCheck, ref canDropRight);

        //if both checks fail, cant drop
        if (!canDropLeft && !canDropRight) return;

        //DROP STARTS HERE
        //remove from all on head
        Transform objectToDrop = allObjectivesOnHead.Pop();
        objectToDrop.GetComponent<Rigidbody2D>().simulated = true;
        //now that removed from head, remove top of fake ones, mm yes
        UpdateFakeColliderOnHead();
        //add back to list of all avaiable for pickup
        allAvailableObjectivesInScene.Add(objectToDrop);
        //change transform
        objectToDrop.SetParent(objectiveContainer);

        //start decaying
        objectToDrop.GetComponent<ObjectiveSelfController>().StartDecaying();

        //find where to place, hardest part hmmm
        //right with fail on left
        bool isGoingLeft = GetComponent<MovementScript>().IsFacingLeft();
        if (isGoingLeft && canDropLeft)
        {
            objectToDrop.transform.position = leftDropPosition.position;
            //apply force in direction of drop
            if (shouldThrow) objectToDrop.GetComponent<Rigidbody2D>().AddForce(Vector2.left * powerThrow);
        }
        //else if (canDropRight)
        //{
        //    objectToDrop.transform.position = rightDropPosition.position;
        //    if (shouldThrow)
        //    {
        //        objectToDrop.GetComponent<Rigidbody2D>().AddForce(Vector2.right * powerThrow);
        //        playerSounds?.PlayThrow();
        //    }
        //}
        //left with fail on right
        else if (!isGoingLeft && canDropRight)
        {
            objectToDrop.transform.position = rightDropPosition.position;
            if (shouldThrow) objectToDrop.GetComponent<Rigidbody2D>().AddForce(Vector2.right * powerThrow);

        }
        //else if (canDropLeft)
        //{
        //    objectToDrop.transform.position = leftDropPosition.position;
        //    if (shouldThrow)
        //    {
        //        objectToDrop.GetComponent<Rigidbody2D>().AddForce(Vector2.left * powerThrow);
        //        playerSounds?.PlayThrow();
        //    }
        //}
        //else, just stays on head

        playerSounds?.PlayThrow();   //null conditional operator '?.': if playerSounds != null, PlayJump(). Probably remove before deploy/release. 
    }

    private void UpdateFakeColliderOnHead()
    {
        for (int i = 0; i < allFakeColliders.Length; i++)
        {
            if (i < allObjectivesOnHead.Count) allFakeColliders[i].SetActive(true);
            else allFakeColliders[i].SetActive(false);
        }
    }

    private void CheckSide(Vector3[] sideToCheck, ref bool sideBool)
    {
        Vector3 origin = transform.position + centerRaycastsOffset;
        sideBool = true;
        for (int i = 0; i < sideToCheck.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, sideToCheck[i], sideToCheck[i].magnitude, layerMaskToOnlyHitForSideChecks);

            if (hit.collider != null)
            {
                //Debug.Log(hit.collider.gameObject.name);
                sideBool = false;
            }


            Debug.DrawLine(origin, origin + sideToCheck[i], (sideBool ? Color.green : Color.red));
        }
        //Debug.Log(sideBool);
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position + centerRaycastsOffset;

        for (int i = 0; i < raycastsLeftCheck.Length; i++)
        {
            Gizmos.DrawLine(origin, origin + raycastsLeftCheck[i]);
        }
        for (int i = 0; i < raycastsRightCheck.Length; i++)
        {
            Gizmos.DrawLine(origin, origin + raycastsRightCheck[i]);
        }
    }
}
