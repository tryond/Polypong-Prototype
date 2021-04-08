using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class BallManager : MonoBehaviour
{
    public Ball ballPrefab;

    public float speed = 5f;
    public int num = 1;
    public float delay = 1f;

    public Ball[] balls;
    
    void Start()
    {
        balls = new Ball[num];
        for (int i = 0; i < num; i++)
        {
            balls[i] = Instantiate(ballPrefab);
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
            rb.velocity = speed * Vector2.down;
            yield return new WaitForSeconds(delay);
        }
    }
}
