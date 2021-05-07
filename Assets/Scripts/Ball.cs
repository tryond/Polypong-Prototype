using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Schema;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Ball : MonoBehaviour
{
    public float damage = 25f;
    public float healing = 10f;

    [CanBeNull] public Side targetSide = null;
    public Vector2 targetLocalPos = Vector2.zero;

    private Vector2 velocity;

    private float timeElapsed = 0f;
    private float timeToTarget = 0f;
    private Vector2 lastBouncePos;

    public float speed;
    public Vector2 direction;

    [CanBeNull] public Post collidingPost;
    [CanBeNull] public Paddle collidingPaddle;
    [CanBeNull] public Side collidingSide;
    
    public bool isColliding;

    public Vector2 lastCollisionPoint;
    
    
    [CanBeNull] public Dictionary<String, IReflector> reflectorMap;

    // public bool Colliding { get => isColliding != null; }

    private Rigidbody2D rb;

    public UnityEvent<Ball> OnBounce = new UnityEvent<Ball>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        velocity = Vector2.zero;
        
        isColliding = false;
        reflectorMap = new Dictionary<String, IReflector>();

        lastCollisionPoint = Vector2.zero;
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        velocity = Vector2.zero;
    }

    // TODO
    private void FixedUpdate()
    {
        ApplyCollisions();
        // TrackTarget();
        
        rb.MovePosition(transform.position + (Time.fixedDeltaTime * (Vector3) velocity));
    }


    private void SetTargetSide(Vector2 direction)
    {
        Debug.Log("Setting target side!");
        
        Debug.DrawLine(transform.position, 100f * direction, Color.blue, 3);
        
        var hits = Physics2D.RaycastAll(transform.position, direction, 100f);
        var sideHit = hits.FirstOrDefault(hit => hit.collider.CompareTag("Side"));
        if (!sideHit)
        {
            Debug.Log("NO HITS!");
            return;
        }
        
        
        
        targetSide = sideHit.transform.gameObject.GetComponent<Side>();
        // targetLocalPos = targetSide.transform.InverseTransformPoint(sideHit.point);
        //
        // var distanceToTarget = Vector2.Distance(sideHit.point,transform.position);
        // timeElapsed = 0f;
        // timeToTarget = distanceToTarget / speed;
        
        targetSide.AddIncomingBall(this);
        velocity = speed * direction;
    }

    private bool ApplyCollisions()
    {
        if (!collidingPost && !collidingPaddle && !collidingSide)
            return false;

        if (collidingSide && (collidingPost || collidingPaddle))
            collidingSide = null;

        var outgoing = Vector2.zero;
        lastCollisionPoint = transform.position;
        
        if (collidingPost)
        {
            Debug.Log("Applying POST");
            outgoing += collidingPost.GetReflection(lastCollisionPoint, velocity);
            collidingPost = null;
        }

        if (collidingPaddle)
        {
            Debug.Log("Applying PADDLE");
            outgoing += collidingPaddle.GetReflection(lastCollisionPoint, velocity);
            collidingPaddle.PaddleHit(this);
            collidingPaddle = null;
        }

        if (collidingSide)
        {
            Debug.Log("Applying SIDE");
            outgoing += collidingSide.GetReflection(lastCollisionPoint, velocity);
            collidingSide.SideHit(this);
            collidingSide = null;
        }

        SetTargetSide(outgoing.normalized);
        
        OnBounce.Invoke(this);
        return true;
    }

    private void TrackTarget()
    {
        if (!targetSide)
            return;

        var targetGlobalPosition = targetSide.transform.TransformPoint(targetLocalPos);

        // if (timeToTarget <= timeElapsed)
        //     return;
        //
        // SetVelocity(1f / (timeToTarget - timeElapsed) * (targetGlobalPosition - transform.position));
        // timeElapsed += Time.fixedDeltaTime;
        
        SetVelocity(speed * (targetGlobalPosition - transform.position).normalized);
    }
    
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var reflector = other.gameObject.GetComponent<IReflector>();
        if (reflector == null)
            return;

        switch (other.tag)
        {
            case "Post":
                collidingPost = other.GetComponent<Post>();
                break;
            case "Paddle":
                collidingPaddle = other.GetComponent<Paddle>();
                break;
            case "Side":
                collidingSide = other.GetComponent<Side>();
                break;
        }
        
        Debug.Log($"adding {other.gameObject.tag}");
    }

    public void SetVelocity(Vector2 velocity)
    {
        speed = velocity.magnitude;
        direction = velocity.normalized;
        this.velocity = velocity;
    }

    // public void Launch(Vector2 velocity)
    // {
    //
    //     Debug.Log($"velocity -> {velocity}");
    //     Debug.Log($"rb -> {rb}");
    //     
    //     // speed = velocity.magnitude;
    //     this.rb.velocity = velocity;
    // }
    //
    // private void CheckStatus()
    // {
    //     rb.velocity = BallManager.instance.round.speed * rb.velocity.normalized;
    //     if (BallManager.instance.balls.Count > BallManager.instance.round.numBalls)
    //     {
    //         this.gameObject.SetActive(false);
    //         BallManager.instance.balls.Remove(this);
    //     }
    // }
    
    
}
