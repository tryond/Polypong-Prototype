using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class BallManager : MonoBehaviour
{
    public static BallManager instance;
    
    public Ball ballPrefab;
    private Ball[] _ballPool;
    public List<Ball> balls;
    
    public bool random = true;
    
    // public float speed = 5f;
    // private int numBalls = 1;
    public float delay = 1f;

    public float damage = 25f;
    public float healing = 5f;

    [Serializable]
    public struct Round {
        public int sidesRemaining;
        public int numBalls;
        public float speed;
    }
    public Round[] rounds;
    private Queue<Round> _rounds = new Queue<Round>();
    
    public Round round;
    private int _roundIndex = 0;

    private int maxBalls;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        balls = new List<Ball>();
        
        foreach (var r in rounds)
            _rounds.Enqueue(r);
        
        round = _rounds.Dequeue();
        
        maxBalls = 0;
        foreach (var round in rounds)
            maxBalls = Math.Max(maxBalls, round.numBalls);
        
        _ballPool = new Ball[maxBalls];
        for (int i = 0; i < maxBalls; i++)
        {
            _ballPool[i] = Instantiate(ballPrefab);
            _ballPool[i].damage = damage;
            _ballPool[i].healing = healing;
            _ballPool[i].gameObject.SetActive(false);
        }

        StartCoroutine(LaunchBalls());
    }

    private IEnumerator LaunchBalls()
    {
        for (int i = 0; i < round.numBalls; i++)
        {
            var ball = GetBall();
            ball.gameObject.SetActive(true);
            balls.Add(ball);
            
            var rb = ball.gameObject.GetComponent<Rigidbody2D>();
            var direction = random ? new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized : Vector2.down;
            
            rb.velocity = round.speed * direction;
            yield return new WaitForSeconds(delay);
        }
    }
    
    public Ball GetBall() {
        for (int i = 0; i < _ballPool.Length; i++) {
            if (!_ballPool[i].gameObject.activeInHierarchy) {
                return _ballPool[i];
            }
        }
        return null;
    }

    public void BallBounced(Ball ball)
    {
        Debug.Log("BOUNCE!");
        
        // if too many active, reset
        if (balls.Count > round.numBalls)
        {
            balls.Remove(ball);
            ball.transform.position = transform.position;
            ball.Reset();
        }
        else
        {
            Debug.Log(balls.Count + " <= " + round.numBalls);
        }

        // otherwise update speed
        var rb = ball.GetComponent<Rigidbody2D>();
        rb.velocity = round.speed * rb.velocity.normalized;
    }
    
    
    public void SetNextRound(int numPlayers)
    {
        Debug.Log($"Setting next round to {numPlayers} players");
        
        if (_rounds.Count <= 0)
            return;

        var nextRound = _rounds.Peek();
        if (numPlayers <= nextRound.sidesRemaining)
            round = _rounds.Dequeue();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Ball"))
            return;

        other.transform.position = transform.position;
    }
}
