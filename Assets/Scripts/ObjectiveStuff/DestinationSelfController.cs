using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationSelfController : MonoBehaviour
{
    public string objectiveTag;

    private SpriteRenderer thisSR;
    private bool isCompleted = false;

    private void Start()
    {
        thisSR = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != objectiveTag) return;
        if (isCompleted) return;

        Debug.Log("Do the thing");
        isCompleted = true;
        DestinationManager.Instance.SetDestinationCompleted(this.gameObject);
        GetComponent<HouseLightAnimator>().animate = true;
        //stop decay and disalbe
        collision.GetComponent<ObjectiveSelfController>().StopDecaying();
        collision.GetComponent<ObjectiveSelfController>().DisableObjectiveAfterXTime();
    }
}
