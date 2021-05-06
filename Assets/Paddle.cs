using System;
using System.Numerics;
using MoreMountains.NiceVibrations;
using UnityEngine;
using UnityEngine.Events;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(BoxCollider2D))]
public class Paddle : MonoBehaviour, IReflector
{
    public float maxNormal = 30f;
    public float maxReflection = 45f;
    
    public float maxSpeed = 10f;
    public float smoothTime = 0.2f;
    
    private Vector2 _normal;

    protected Vector2 targetPosition;
    private Vector3 velocity;
    
    private Rigidbody2D rb;

    [SerializeField] UnityEvent<Ball> OnPaddleHit = new UnityEvent<Ball>();
    [SerializeField] UnityEvent<Side, Ball> OnPaddleDeath = new UnityEvent<Side, Ball>();

    private float baseHorizontalScale;
    public float maxHealth = 100f;
    private float health;
    
    private CapsuleCollider2D collider;
    
    public virtual void Start()
    {
        _normal = transform.up;

        targetPosition = transform.position;
        velocity = Vector3.zero;
        rb = GetComponent<Rigidbody2D>();

        baseHorizontalScale = transform.localScale.x;
        health = maxHealth;

        collider = GetComponent<CapsuleCollider2D>();
    }

    public void FixedUpdate()
    {
        // ensure target position is along local, horizontal axis
        var parent = transform.parent;
        var localTarget = parent.InverseTransformPoint(targetPosition);
        targetPosition = parent.TransformPoint(new Vector2(localTarget.x, 0f));
        
        // smooth move rb to next position, updating velocity
        Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, maxSpeed);
        rb.velocity = velocity;
        
        // determine normal direction
        var horizontalVelocity = Vector3.Dot(transform.right, velocity);
        var relativeSpeed = Mathf.Clamp(horizontalVelocity / maxSpeed, -1f, 1f);
        _normal = Quaternion.Euler(0f, 0f, -1f * relativeSpeed * maxNormal) * transform.up;

        // TODO: debug
        Debug.DrawRay(transform.position, _normal, Color.green, Time.deltaTime);
    }

    // public void OnTriggerEnter2D(Collider2D other)
    // {
    //     var ball = other.gameObject.GetComponent<Ball>();
    //     if (ball == null || ball.collidingWith != collider)
    //         return;
    //
    //     OnPaddleHit.Invoke(ball);
    // }


    // private void ReflectBall(Ball ball)
    // {
    //     // check that ball is moving towards paddle
    //     if (Vector3.Dot(_normal, ball.velocity) >= 0)
    //         return;
    //     
    //     var incoming = ball.velocity;
    //     var outgoing = Vector3.Reflect(incoming, _normal);
    //     
    //     var difference = Vector3.SignedAngle(outgoing, transform.up, transform.forward);
    //     difference = Mathf.Clamp(difference, -maxReflection, maxReflection);
    //     outgoing = Quaternion.Euler(0f, 0f, -difference) * transform.up * incoming.magnitude;
    //     
    //     ball.velocity = outgoing;
    //     MMVibrationManager.Haptic(HapticTypes.RigidImpact);
    // }


    public Vector2 GetReflection(Vector2 contactPoint, Vector2 direction)
    {
        var incoming = direction.normalized;
        
        // check that ball is moving towards paddle
        if (Vector3.Dot(_normal, incoming) >= 0)
            return incoming;
        
        var outgoing = Vector3.Reflect(incoming, _normal);
        
        var difference = Vector3.SignedAngle(outgoing, transform.up, transform.forward);
        difference = Mathf.Clamp(difference, -maxReflection, maxReflection);
        outgoing = Quaternion.Euler(0f, 0f, -difference) * transform.up;

        return outgoing.normalized;
    }

    public void SetTargetPosition(Vector3 target) => targetPosition = target;

    public void TakeDamage(Ball ball)
    {
        health = Mathf.Max(health - ball.damage, 0f);
        transform.localScale = new Vector3( (health / maxHealth) * baseHorizontalScale, transform.localScale.y, transform.localScale.z );
        
        if (health <= 0f)
            OnPaddleDeath.Invoke(transform.parent.GetComponent<Side>(), ball);
    }

    public void Heal(Ball ball)
    {
        health = Mathf.Min(health + ball.healing, maxHealth);
        transform.localScale = new Vector3( (health / maxHealth) * baseHorizontalScale, transform.localScale.y, transform.localScale.z );
    }

    public void PaddleHit(Ball ball)
    {
        OnPaddleHit.Invoke(ball);
    }
    
}
