using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveSelfController : MonoBehaviour
{
    public Text textTimer;
    public float timeForDecay = 10f;
    public bool isDecaying = false;

    private float timerDecaying = 0f;

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
}
