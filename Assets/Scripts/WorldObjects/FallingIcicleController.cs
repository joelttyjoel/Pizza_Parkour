using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation.Examples;

public class FallingIcicleController : MonoBehaviour
{
    public AudioClip fallingSoundLoop;
    public AudioClip damagePlayer;
    public float timeBeforeFall;
    public float timeBeforeRespawn;
    public float distanceCheckForPlayer;
    public float timeToFallFor;

    private Vector3 startPosition;
    private Rigidbody2D thisRB;
    private SpriteRenderer thisSR;
    private PathFollower thisPath;
    private bool isFalling = false;
    private bool canDamage = false;
    private bool isHit;
    private bool hasBeenHit = false;
    private float timeBeingHit;
    private AudioSource thisAudioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        thisRB = GetComponent<Rigidbody2D>();
        thisSR = GetComponentInParent<SpriteRenderer>();
        startPosition = transform.position;
        isFalling = false;
        thisPath = GetComponent<PathFollower>();
        thisAudioSource = GetComponent<AudioSource>();

        thisPath.enabled = false;
    }

    private void Update()
    {
        if (isFalling) return;

        //check if should fall
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPosition, Vector2.down, distanceCheckForPlayer);

        isHit = false;
        for(int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.tag == "Player" && !hits[i].collider.isTrigger)
            {
                isHit = true;
            }
        }
        
        if(isHit)
        {
            timeBeingHit -= Time.deltaTime;
            thisPath.enabled = true;
            if(!hasBeenHit)
            {
                hasBeenHit = true;
                thisAudioSource.clip = fallingSoundLoop;
                thisAudioSource.Play();
            }
        }
        else
        {
            hasBeenHit = false;
            thisAudioSource.Stop();
            timeBeingHit = timeBeforeFall;
            thisPath.enabled = false;
            transform.position = startPosition;
        }

        if(timeBeingHit < 0f)
        {
            hasBeenHit = false;
            thisAudioSource.Stop();
            isFalling = true;
            canDamage = true;
            thisRB.simulated = true;
            thisPath.enabled = false;
            StartCoroutine(RespawnTimer());
        }
    }

    private IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(timeToFallFor);
        thisRB.simulated = false;
        canDamage = false;
        thisSR.enabled = false;
        yield return new WaitForSeconds(timeBeforeRespawn);
        thisRB.velocity = Vector3.zero;
        transform.position = startPosition;
        timeBeingHit = timeBeforeFall;
        thisPath.enabled = false;
        isFalling = false;
        thisSR.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canDamage) return;
        //if player, do something
        if (collision.tag == "Player")
        {
            Debug.Log("Damage Player");
            //for now drop box because fanni
            GameObject.Find("Player").GetComponent<InteractObjectiveController>().DropTopObjective(false);
            thisAudioSource.PlayOneShot(damagePlayer);
            canDamage = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -distanceCheckForPlayer));
    }
}
