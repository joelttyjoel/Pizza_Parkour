using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public Vector3 startXPosition;
    public Vector3 endXPosition;
    public float speed;
    public float headStartInSeconds;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(startXPosition.x, transform.position.y, transform.position.x);

        transform.position = new Vector3(transform.position.x + headStartInSeconds * speed, transform.position.y, transform.position.x);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x + Time.deltaTime * speed, transform.position.y, transform.position.x);

        if(speed > 0f)
        {
            if (transform.position.x > endXPosition.x) transform.position = new Vector3(startXPosition.x, transform.position.y, transform.position.x);
        }
        else if(speed < 0f)
        {
            if (transform.position.x < endXPosition.x) transform.position = new Vector3(startXPosition.x, transform.position.y, transform.position.x);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(startXPosition, endXPosition);
        Gizmos.color = Color.green;
        Gizmos.DrawCube(startXPosition, new Vector3(1f, 1f, 1f));
        Gizmos.color = Color.red;
        Gizmos.DrawCube(endXPosition, new Vector3(1f, 1f, 1f));
    }
}
