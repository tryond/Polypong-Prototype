using UnityEngine;
using UnityEngine.Events;

public class Side : MonoBehaviour
{
    [SerializeField] UnityEvent<Ball> OnSideHit = new UnityEvent<Ball>();

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Ball"))
            return;

        var ball = other.gameObject.GetComponent<Ball>();
        OnSideHit.Invoke(ball);
    }
}