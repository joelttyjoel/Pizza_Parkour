using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RosettController : MonoBehaviour
{
    public Transform parentTransform;
    private TargetJoint2D joint;

    private void Start()
    {
        //parentTransform = GetComponentInParent<Transform>();
        joint = GetComponent<TargetJoint2D>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        joint.target = parentTransform.position;
    }
}
