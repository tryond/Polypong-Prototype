using UnityEngine;

public class Side : MonoBehaviour
{
    [SerializeField] UnityFloatEvent OnSideHit = new UnityFloatEvent();

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Ball"))
            return;

        var ball = other.gameObject.GetComponent<Ball>();
        OnSideHit.Invoke(ball.damage);
    }
}