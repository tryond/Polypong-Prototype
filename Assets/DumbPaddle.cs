using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class DumbPaddle : Paddle
{
    private bool goingRight = true;
    
    public override void Start()
    {
        base.Start();
        
        var targetPos = 1000f * transform.right;
        SetTargetPosition(targetPos);
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Vertex"))
            return;

        ReverseDirection();
    }

    private void ReverseDirection()
    {
        var direction = goingRight ? -transform.right : transform.right;
        SetTargetPosition(100f * direction);
        goingRight = !goingRight;
    }
}