using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation.Examples;

public class DecayingPlatformController : MonoBehaviour
{
    public string tagColliderToStartFallOn;
    public float timeBeforeDecay = 3f;
    public float timeBeforeRespawn = 5f;

    private bool isDecayed = false;
    private SpriteRenderer thisSR;
    private EdgeCollider2D thisC;
    private PathFollower thisAnimation;
    private AudioSource thisAudioSource;

    private void Start()
    {
        thisSR = GetComponentInChildren<SpriteRenderer>();
        thisC = GetComponent<EdgeCollider2D>();
        thisAnimation = GetComponentInChildren<PathFollower>();
        thisAudioSource = GetComponent<AudioSource>();

        thisAnimation.enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag != tagColliderToStartFallOn) return;
        if (isDecayed) return;

        isDecayed = true;
        StartCoroutine(DecayProcess());
    }

    private IEnumerator DecayProcess()
    {
        thisAudioSource.Play();
        //Debug.Log("Start decaying");
        //if animation start here
        thisAnimation.enabled = true;
        //start timer
        yield return new WaitForSeconds(timeBeforeDecay);
        //disable platform because decayed mm yes
        thisSR.enabled = false;
        thisC.enabled = false;
        thisAudioSource.Stop();
        //start respawn process
        StartCoroutine(RespawnProcess());
    }

    private IEnumerator RespawnProcess()
    {
        //animation stop move and reset sprite to 000
        thisAnimation.enabled = false;
        thisSR.transform.localPosition = new Vector3(0, 0, 0);

        yield return new WaitForSeconds(timeBeforeRespawn);
        thisSR.enabled = true;
        thisC.enabled = true;
        isDecayed = false;
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
}
