using System;
using System.Numerics;
using JetBrains.Annotations;
using Shapes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Line))]
[ExecuteAlways] public class Side : ImmediateModeShapeDrawer, IReflector
{
    public float Length { get; set; }

    private Line line;
    
    private BoxCollider2D collider;
    private float colliderBaseSizeY;
    private float colliderBaseOffsetY;
    
    [CanBeNull] public Paddle paddle;
    
    [SerializeField] UnityEvent<Ball> OnSideHit = new UnityEvent<Ball>();

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        colliderBaseSizeY = collider.size.y;
        colliderBaseOffsetY = collider.offset.y;

        line = GetComponent<Line>();
        
        if (!paddle)
            return;

        // TODO: this needs to be adjusted back when paddle is removed
        // var paddleHeight = paddle.GetComponent<BoxCollider2D>().size.y;
        // collider.offset = new Vector2(0f, colliderBaseOffsetY - (0.55f * paddleHeight));
    }

    public void SetColors(Color startColor, Color endColor)
    {
        line.ColorStart = startColor;
        line.ColorEnd = endColor;
    }
    
    public void SetLength(float length)
    {
        Length = length;
        
        line.Start = new Vector2(-length / 2f, 0f);
        line.End = new Vector2(length / 2f, 0f);
        
        collider.size = new Vector2(length, colliderBaseSizeY);
    }

    public void SetPaddle([CanBeNull] Paddle paddle)
    {
        if (this.paddle && !paddle)
            Destroy(this.paddle.gameObject);
        
        this.paddle = paddle;
        
        // update the goal y offset according to the height of the paddle if not null
        // var colliderOffsetY = paddle ? 0.55f * paddle.GetComponent<BoxCollider2D>().size.y : 0f;
        // collider.offset = new Vector2(0f, colliderBaseOffsetY - colliderOffsetY);
    }

    // public void OnTriggerEnter2D(Collider2D other)
    // {
    //     var ball = other.gameObject.GetComponent<Ball>();
    //     if (ball == null || ball.collidingWith != collider)
    //         return;
    //
    //     // var headedTowards = Vector2.Dot(ball.direction, transform.up) <= 0;
    //     // if (!headedTowards)
    //     //     return;
    //     
    //     OnSideHit.Invoke(ball);
    // }

    public Vector2 GetReflection(Vector2 contactPoint, Vector2 direction)
    {
        return Vector2.Reflect(direction.normalized, transform.up);
    }

    public void SideHit(Ball ball)
    {
        OnSideHit.Invoke(ball);
    }
}