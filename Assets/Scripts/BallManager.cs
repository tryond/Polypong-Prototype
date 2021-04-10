using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class BallManager : MonoBehaviour
{
    public Ball ballPrefab;

    public bool random = false;
    
    public float speed = 5f;
    public int num = 1;
    public float delay = 1f;

    public float damage = 25f;
    public float healing = 5f;
    
    public Ball[] balls;
    
    void Start()
    {
        balls = new Ball[num];
        for (int i = 0; i < num; i++)
        {
            balls[i] = Instantiate(ballPrefab);
            balls[i].damage = damage;
            balls[i].healing = healing;
            balls[i].gameObject.SetActive(false);
        }

        StartCoroutine(LaunchBalls());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Ball"))
            return;
        
        var rb = other.gameObject.GetComponent<Rigidbody2D>();
        rb.velocity = -rb.velocity;
    }
    
    private IEnumerator LaunchBalls()
    {
        foreach (var ball in balls)
        {
            ball.gameObject.SetActive(true);
            var rb = ball.gameObject.GetComponent<Rigidbody2D>();
            
            var direction = random ? new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized : Vector2.down;
            
            rb.velocity = speed * direction;
            yield return new WaitForSeconds(delay);
        }
    }
}
