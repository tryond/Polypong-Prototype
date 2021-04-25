using System;
using JetBrains.Annotations;
using Shapes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Line))]
[ExecuteAlways] public class Side : ImmediateModeShapeDrawer
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
        var paddleHeight = paddle.GetComponent<BoxCollider2D>().size.y;
        collider.offset = new Vector2(0f, colliderBaseOffsetY + (1.5f * paddleHeight));
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

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Ball"))
            return;

        var ball = other.gameObject.GetComponent<Ball>();
        OnSideHit.Invoke(ball);
    }
}