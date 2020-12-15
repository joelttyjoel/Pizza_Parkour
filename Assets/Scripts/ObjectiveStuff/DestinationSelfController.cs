using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationSelfController : MonoBehaviour
{
    public List<AudioClip> audioClips;
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

        //set random audio clip for when done
        int random = Random.Range(0, audioClips.Count);
        GetComponent<AudioSource>().clip = audioClips[random];
        GetComponent<AudioSource>().Play();

        //Debug.Log("Do the thing");
        isCompleted = true;
        DestinationManager.Instance.SetDestinationCompleted(this.gameObject);
        GetComponent<HouseLightAnimator>().animate = true;
        //stop decay and disalbe
        collision.GetComponent<ObjectiveSelfController>().StopDecaying();
        collision.GetComponent<ObjectiveSelfController>().DisableObjectiveAfterXTime();
    }
}
