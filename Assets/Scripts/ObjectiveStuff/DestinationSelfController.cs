using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationSelfController : MonoBehaviour
{
    public GameObject objectiveNeeded;
    public GameObject messageIfWrong;
    public string objectiveTag;
    public Color colorOnFailed;
    public Color colorOnComplete;

    private SpriteRenderer thisSR;
    private int wrongObjectivesInHouse = 0;

    private void Start()
    {
        thisSR = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetInstanceID() == objectiveNeeded.GetInstanceID())
        {
            DestinationManager.Instance.SetDestinationCompleted(this.gameObject);
            thisSR.color = colorOnComplete;
        }
        else if(collision.gameObject.tag == objectiveTag)
        {
            wrongObjectivesInHouse++;
        }

        if (wrongObjectivesInHouse > 0) messageIfWrong.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
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
