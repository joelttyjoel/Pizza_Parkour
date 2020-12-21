using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public float effectLevel = 1f;

    private float startPos;
    private Transform camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main.transform;
        startPos = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = camera.position.x * effectLevel;

        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);
    }
}
