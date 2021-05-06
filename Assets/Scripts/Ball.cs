using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

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
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        velocity = Vector2.zero;
    }

    // TODO
    private void FixedUpdate()
    {
        var collisionsApplied = ApplyCollisions();
        if (collisionsApplied) isColliding = true;

        rb.MovePosition(transform.position + (Time.fixedDeltaTime * (Vector3) velocity));
        // rb.MovePosition((Time.fixedDeltaTime * (Vector3) velocity) + transform.position);


        // if (!targetSide)
        //     return;
        //
        // var targetGlobalPos = targetSide.transform.TransformPoint(targetLocalPos);
        // rb.velocity = (timeToTarget - timeElapsed) * Time.fixedDeltaTime * (targetGlobalPos - transform.position);
        //
        // timeElapsed += Time.fixedDeltaTime;
    }

    // private void OnCollisionExit2D(Collision2D other)
    // {
    //     OnBounce.Invoke(GetComponent<Ball>());
    //     SetTargetSide();
    // }
    //
    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     OnBounce.Invoke(GetComponent<Ball>());
    //     SetTargetSide();
    // }

    // private void SetTargetSide()
    // {
    //     Debug.Log("Setting target side!");
    //     
    //     Debug.DrawLine(transform.position, 100f * (transform.up + transform.position), Color.blue, 3);
    //     
    //     var hits = Physics2D.RaycastAll(transform.position, transform.up, 100f);
    //     var sideHit = hits.FirstOrDefault(hit => hit.collider.CompareTag("Side"));
    //     if (!sideHit)
    //     {
    //         Debug.Log("NO HITS!");
    //         return;
    //     }
    //     
    //     targetSide = sideHit.transform.gameObject.GetComponent<Side>();
    //     targetLocalPos = targetSide.transform.InverseTransformPoint(sideHit.point);
    //
    //     var distanceToTarget = Vector2.Distance(sideHit.point,transform.position);
    //     timeElapsed = 0f;
    //     timeToTarget = distanceToTarget / speed;
    // }

    private bool ApplyCollisions()
    {
        if (!collidingPost && !collidingPaddle && !collidingSide)
            return false;

        if (collidingSide && (collidingPost || collidingPaddle))
            collidingSide = null;

        var outgoing = Vector2.zero;

        if (collidingPost)
        {
            Debug.Log("Applying POST");
            outgoing += collidingPost.GetReflection(transform.position, velocity);
            collidingPost = null;
        }

        if (collidingPaddle)
        {
            Debug.Log("Applying PADDLE");
            outgoing += collidingPaddle.GetReflection(transform.position, velocity);
            collidingPaddle.PaddleHit(this);
            collidingPaddle = null;
        }

        if (collidingSide)
        {
            Debug.Log("Applying SIDE");
            outgoing += collidingSide.GetReflection(transform.position, velocity);
            collidingSide.SideHit(this);
            collidingSide = null;
        }

        velocity = speed * outgoing.normalized;
        OnBounce.Invoke(this);
        return true;
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

    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     var reflector = other.gameObject.GetComponent<IReflector>();
    //     if (reflector == null)
    //         return;
    //
    //     reflectorMap.Remove(reflector.GetHashCode());
    //     if (reflectorMap.Count <= 0)
    //         isColliding = false;
    // }

    private void Bounce(Vector2 direction)
    {
        velocity = direction * speed;
        OnBounce.Invoke(GetComponent<Ball>());
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
        velocity = this.speed * direction;
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
        velocity = speed * this.direction;
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
