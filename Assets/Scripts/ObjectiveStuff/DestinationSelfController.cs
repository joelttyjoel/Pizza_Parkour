using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationSelfController : MonoBehaviour
{
    [Header("IF NO OBJECT SET, CAN TAKE ANY OBJECT")]
    public GameObject objectiveNeeded;
    public GameObject messageIfWrong;
    public string objectiveTag;
    public Color colorOnFailed;
    public Color colorOnComplete;

    private SpriteRenderer thisSR;
    private int rightObjectivesInHouse = 0;
    private int wrongObjectivesInHouse = 0;

    private void Start()
    {
        thisSR = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != objectiveTag) return;

        //if no specific objective, only do if not yet completed
        if (objectiveNeeded == null)
        {
            rightObjectivesInHouse++;
            //was 0, became 1, only good value
            if(rightObjectivesInHouse == 1)
            {
                DestinationManager.Instance.SetDestinationCompleted(this.gameObject);
                thisSR.color = colorOnComplete;
                //stop decay
                collision.GetComponent<ObjectiveSelfController>().StopDecaying();
            }
            return;
        }

        //if need objective
        if (collision.gameObject.GetInstanceID() == objectiveNeeded.GetInstanceID())
        {
            DestinationManager.Instance.SetDestinationCompleted(this.gameObject);
            thisSR.color = colorOnComplete;
            //stop decay
            collision.GetComponent<ObjectiveSelfController>().StopDecaying();
        }
        else if(collision.gameObject.tag == objectiveTag)
        {
            wrongObjectivesInHouse++;
        }

        if (wrongObjectivesInHouse > 0) messageIfWrong.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag != objectiveTag) return;

        //if no specific objective, only do if completed
        if (objectiveNeeded == null)
        {
            rightObjectivesInHouse--;
            //was 0, became 1, only good value
            if (rightObjectivesInHouse == 0)
            {
                DestinationManager.Instance.SetDestinationFailed(this.gameObject);
                thisSR.color = colorOnFailed;
            }
            return;
        }

        if (collision.gameObject.GetInstanceID() == objectiveNeeded.GetInstanceID())
        {
            DestinationManager.Instance.SetDestinationFailed(this.gameObject);
            thisSR.color = colorOnFailed;
        }
        else if (collision.gameObject.tag == objectiveTag)
        {
            wrongObjectivesInHouse--;
        }

        if (wrongObjectivesInHouse < 1) messageIfWrong.SetActive(false);
    }
}
