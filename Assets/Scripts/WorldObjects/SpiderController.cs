using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    [Header("references and shit")]
    public Transform spiderBaseTransform;
    [Header("settings")]
    public float spiderNormalDistance;
    public Vector3 spiderSenseVectorsOriginOffset;
    public Vector3[] spiderSenseVectors;
    public LayerMask spiderSenceLayerMaskToHit;
    public float spiderGoDownToGrabSpeed;
    public float spiderAttackCooldown;
    [Header("Idle settings")]
    public Vector3 spiderIdleLeftForce;
    public Vector3 spiderIdleRightForce;
    public float spiderIdleSideToSideTime;

    private bool playerIsInSense = false;
    private float currentSpiderGrabDistance;
    private float currentIdleUpDownMovementFactor;
    private LineRenderer spiderThread;
    private DistanceJoint2D spiderDistanceJoint;
    private Rigidbody2D thisRB;
    private bool canAttack = true;

    // Start is called before the first frame update
    void Start()
    {
        spiderThread = GetComponent<LineRenderer>();
        thisRB = GetComponent<Rigidbody2D>();
        spiderDistanceJoint = GetComponent<DistanceJoint2D>();
        
        spiderDistanceJoint.distance = spiderNormalDistance;
        UpdateSpiderThread();

        StartCoroutine(SpiderIdleSideToSide());
    }

    // Update is called once per frame
    void Update()
    {
        CheckForPlayerInSense();

        //if can see player, increase distance of string until grab box
        if(playerIsInSense && canAttack)
        {
            currentSpiderGrabDistance += Time.deltaTime * spiderGoDownToGrabSpeed;
        }
        //if cant see, decrease distance
        else
        {
            currentSpiderGrabDistance -= Time.deltaTime * spiderGoDownToGrabSpeed;
            if (currentSpiderGrabDistance < 0f) currentSpiderGrabDistance = 0f;
            //Debug.Log(currentSpiderGrabDistance);
        }

        UpdateSpiderThread();
    }

    private void CheckForPlayerInSense()
    {
        Vector3 origin = transform.position + spiderSenseVectorsOriginOffset;
        playerIsInSense = false;
        for (int i = 0; i < spiderSenseVectors.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, spiderSenseVectors[i], spiderSenseVectors[i].magnitude, spiderSenceLayerMaskToHit);
            if (hit.collider != null && hit.collider.gameObject.tag == "Player")
            {
                if(hit.collider.GetComponent<InteractObjectiveController>().GetObjectivesOnHeadCount() > 0) playerIsInSense = true;
            }

            Debug.DrawLine(origin, origin + spiderSenseVectors[i], (playerIsInSense ? Color.green : Color.red));
        }
    }
    
    private void UpdateSpiderThread()
    {
        spiderDistanceJoint.distance = spiderNormalDistance + currentSpiderGrabDistance;

        spiderThread.SetPosition(0, spiderBaseTransform.position);
        spiderThread.SetPosition(1, transform.position);
    }

    private IEnumerator SpiderIdleSideToSide()
    {
        while(true)
        {
            yield return new WaitForSeconds(spiderIdleSideToSideTime);
            bool direction = (Random.value < 0.5f);
            thisRB.AddForce(direction ? spiderIdleLeftForce : spiderIdleRightForce);
        }
    }

    private IEnumerator SpiderAttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(spiderAttackCooldown);
        canAttack = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag != "Player") return;
        if (!canAttack) return;

        canAttack = false;
        GetComponent<AudioSource>().Play();
        GameObject.Find("Player").GetComponent<InteractObjectiveController>().DropTopObjective(false);
        StartCoroutine(SpiderAttackCooldown());
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position + spiderSenseVectorsOriginOffset;

        for (int i = 0; i < spiderSenseVectors.Length; i++)
        {
            Gizmos.DrawLine(origin, origin + spiderSenseVectors[i]);
        }
    }
}
