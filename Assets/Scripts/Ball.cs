using System;
using UnityEngine;
using UnityEngine.Events;

public class Ball : MonoBehaviour
{
    public float damage = 25f;
    public float healing = 10f;

    private Rigidbody2D rb;

    public UnityEvent<Ball> OnBounce = new UnityEvent<Ball>();
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        rb.velocity = Vector2.zero;
    }
    
    // private void OnCollisionExit2D(Collision2D other) => OnBounce.Invoke(this);

    // private void OnTriggerExit2D(Collider2D other) => OnBounce.Invoke(this);

    private void CheckStatus()
    {
        rb.velocity = BallManager.instance.round.speed * rb.velocity.normalized;
        if (BallManager.instance.balls.Count > BallManager.instance.round.numBalls)
        {
            this.gameObject.SetActive(false);
            BallManager.instance.balls.Remove(this);
        }
    }
    
    
}
