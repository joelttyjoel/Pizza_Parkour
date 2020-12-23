using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnableImageOnEnterTrigger : MonoBehaviour
{
    [Header("Put trigger on same object as script")]
    public GameObject thingToEnable;
    public GameObject parentObject;
    public bool playAudio;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            parentObject.transform.Find(thingToEnable.name).gameObject.SetActive(true);

            if (playAudio) GetComponent<AudioSource>().Play();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            parentObject.transform.Find(thingToEnable.name).gameObject.SetActive(false);
        }
    }
}
