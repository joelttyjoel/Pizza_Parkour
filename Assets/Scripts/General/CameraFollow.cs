using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float speed;
    public Vector2 upperLeftCornerBounds;
    public Vector2 lowerRightCornerBounds;

    private float startingZ;

    private void Start()
    {
        startingZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = target.transform.position;
        float step = speed * Time.deltaTime;
        //cross left bound
        if (targetPos.x < transform.position.x + upperLeftCornerBounds.x)
        {
            transform.position = new Vector3(transform.position.x - ((transform.position.x + upperLeftCornerBounds.x) - targetPos.x), transform.position.y, startingZ);
        }
        //cross right bound
        if (targetPos.x > transform.position.x + lowerRightCornerBounds.x)
        {
            transform.position = new Vector3(transform.position.x + (targetPos.x - (transform.position.x + lowerRightCornerBounds.x)), transform.position.y, startingZ);
        }
        //cross top bound
        if (targetPos.y > transform.position.y + upperLeftCornerBounds.y)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + (targetPos.y - (transform.position.y + upperLeftCornerBounds.y)), startingZ);
        }
        //cross bottom bound
        if (targetPos.y < transform.position.y + lowerRightCornerBounds.y)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - ((transform.position.y + lowerRightCornerBounds.y) - targetPos.y), startingZ);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 cameraPos = transform.position;
        //upper line
        Gizmos.DrawLine(new Vector3(cameraPos.x + upperLeftCornerBounds.x, cameraPos.y + upperLeftCornerBounds.y, 0), new Vector3(cameraPos.x + lowerRightCornerBounds.x, cameraPos.y + upperLeftCornerBounds.y, 0));
        //left side
        Gizmos.DrawLine(new Vector3(cameraPos.x + upperLeftCornerBounds.x, cameraPos.y + upperLeftCornerBounds.y, 0), new Vector3(cameraPos.x + upperLeftCornerBounds.x, cameraPos.y + lowerRightCornerBounds.y, 0));
        //right side
        Gizmos.DrawLine(new Vector3(cameraPos.x + lowerRightCornerBounds.x, cameraPos.y + lowerRightCornerBounds.y, 0), new Vector3(cameraPos.x + lowerRightCornerBounds.x, cameraPos.y + upperLeftCornerBounds.y, 0));
        //bottom line
        Gizmos.DrawLine(new Vector3(cameraPos.x + lowerRightCornerBounds.x, cameraPos.y + lowerRightCornerBounds.y, 0), new Vector3(cameraPos.x + upperLeftCornerBounds.x, cameraPos.y + lowerRightCornerBounds.y, 0));
    }
}
