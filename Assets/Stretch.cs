using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Stretch : MonoBehaviour
{

    public Post left;
    public Post right;
    
    public GameObject scaleX;
    
    [SerializeField] private HingeJoint2D _leftJoint;
    [SerializeField] private HingeJoint2D _rightJoint;

    // Start is called before the first frame update
    void Start()
    {
        _leftJoint.connectedBody = left.GetComponent<Rigidbody2D>();
        _rightJoint.connectedBody = right.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    // void FixedUpdate()
    // {
    //     var leftPos = left.transform.position;
    //     var rightPos = right.transform.position;
    //
    //     var scaleTransform = scaleX.transform;
    //
    //     scaleTransform.position = Vector3.Lerp(leftPos, rightPos, 0.5f);
    //     
    //     var distance = Vector3.Distance(left.transform.position, right.transform.position);
    //     scaleTransform.localScale = new Vector3(distance, 1f, 1f);
    // }

    private void LateUpdate()
    {
        var leftPos = left.transform.position;
        var rightPos = right.transform.position;

        var scaleTransform = scaleX.transform;

        scaleTransform.position = Vector3.Lerp(leftPos, rightPos, 0.5f);
        
        var distance = Vector3.Distance(left.transform.position, right.transform.position);
        scaleTransform.localScale = new Vector3(distance, scaleTransform.localScale.y, scaleTransform.localScale.z);
    }
}
