using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Spine : MonoBehaviour
{
    public float angle = 0f;
    public RelativeJoint2D[] joints;

    private void FixedUpdate()
    {
        foreach (var joint in joints)
            joint.angularOffset = angle;
    }
}
