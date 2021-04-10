using Vector3 = UnityEngine.Vector3;

public class EnemyPaddle : Paddle
{
    public BallManager ballManager;
    
    // TODO: determine position based on incoming balls

    public void FixedUpdate()
    {
        var nextPosition = DeterminePosition();
        base.SetTargetPosition(nextPosition);
        base.FixedUpdate();
    }
    
    private Vector3 DeterminePosition()
    {
        if (!ballManager || ballManager.balls.Length <= 0)
            return transform.position;
        
        Vector3 nearestBallPosition = Vector3.positiveInfinity;
        
        foreach (var ball in ballManager.balls)
            if (ball.transform.localPosition.magnitude < nearestBallPosition.magnitude)
                nearestBallPosition = ball.transform.localPosition;
    
        return nearestBallPosition;
    }
}