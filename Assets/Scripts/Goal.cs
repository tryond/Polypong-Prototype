using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnityFloatEvent : UnityEvent<float>
{
}
public class Goal : MonoBehaviour
{
    [SerializeField] UnityFloatEvent OnGoalHit = new UnityFloatEvent();

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Ball"))
            return;

        var ball = other.gameObject.GetComponent<Ball>();
        OnGoalHit.Invoke(ball.damage);
    }
}
