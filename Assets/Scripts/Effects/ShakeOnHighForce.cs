using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MilkShake;

public class ShakeOnHighForce : MonoBehaviour
{
    public float shakeOnForce;
    public ShakePreset shakePreset;

    private Rigidbody2D thisRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        thisRigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //is expencive to do every time but ey for testing only
        if(collision.relativeVelocity.magnitude > shakeOnForce)
        {
            Shaker.ShakeAll(shakePreset);
        }

        //Debug.Log(collision.relativeVelocity.magnitude);
    }
}
