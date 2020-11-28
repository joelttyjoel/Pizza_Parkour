using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveSelfController : MonoBehaviour
{
    public Text textTimer;
    public float timeForDisable = 0.5f;
    public float timeForDecay = 10f;
    public bool isDecaying = false;

    private float timerDecaying = 0f;
    private bool isBeingDissabled = false;

    // Update is called once per frame
    void Update()
    {
        if (isDecaying)
        {
            timerDecaying -= Time.deltaTime;
            textTimer.text = ((int)(timerDecaying)).ToString();

            if (timerDecaying < 0f)
            {
                //old remove box
                //GameObject.Find("TempPlayer").GetComponent<InteractObjectiveController>().RemoveObjectiveFromWorld(this.transform);
                //new reset level
                SceneController.Instance.ResetCurrentLevel();
            }
        }
    }

    public void StartDecaying()
    {
        isDecaying = true;
        textTimer.enabled = true;
    }
    public void StopDecaying()
    {
        isDecaying = false;
        textTimer.enabled = false;
        timerDecaying = timeForDecay;
    }

    public void DisableObjectiveAfterXTime()
    {
        isBeingDissabled = true;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -5f);
        GetComponent<SpriteRenderer>().sortingOrder = 2;
        GameObject.FindGameObjectWithTag("Player").GetComponent<InteractObjectiveController>().DisableObjectInWorld(this.transform);
        StartCoroutine(DisableTimer());
    }

    private IEnumerator DisableTimer()
    {
        yield return new WaitForSeconds(timeForDisable);
        GetComponent<Rigidbody2D>().simulated = false;
        isBeingDissabled = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isBeingDissabled) return;

        //if (collision.collider.tag == "Player" || collision.collider.tag == "Objective")
        if (collision.collider.tag != "World")
        {
            Physics2D.IgnoreCollision(collision.collider, this.GetComponent<BoxCollider2D>());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBeingDissabled) return;

        if (collision.collider.tag != "World")
        {
            Physics2D.IgnoreCollision(collision.collider, this.GetComponent<BoxCollider2D>());
        }
    }
}
