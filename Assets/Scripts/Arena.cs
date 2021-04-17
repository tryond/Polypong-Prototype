using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[System.Serializable]
public class UnityIntEvent : UnityEvent<int>
{
}

public class Arena : MonoBehaviour
{
    [SerializeField] float startTransitionTime = 0.5f;
    [SerializeField] private float endTransitionTime = 3f;
    
    [SerializeField] int numPlayers;
    [SerializeField] float sideLength;
    
    [SerializeField] private Side enemySidePrefab;

    public BallManager bm;
    
    public Side mainSide;
    private List<Side> sides;

    public Side sidePrefab;
    private Side _leftSide;
    private Side _rightSide;

    private Polygon polygon;
    private Coroutine currentGoalTransition = null;

    public UnityEvent<int> OnNumPlayersChanged = new UnityEvent<int>();
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var gizmoPolygon = new Polygon(numPlayers, sideLength);
        foreach ((Vector3 from, Vector3 to) in gizmoPolygon.Positions)
        {
            Gizmos.DrawLine(from, to);
        }
    }

    private void Start()
    {
        _leftSide = Instantiate(sidePrefab, -10f * Vector3.right, Quaternion.identity);
        _leftSide.gameObject.SetActive(false);
        
        _rightSide = Instantiate(sidePrefab, 10f * Vector3.right, Quaternion.identity);
        _rightSide.gameObject.SetActive(false);

        // create polygon from which to find sector positions
        if (numPlayers == 2)
            polygon = new Polygon(4, sideLength);
        else
            polygon = new Polygon(numPlayers, sideLength);
        
        // create goals
        sides = new List<Side>();
        
        // add player (if defined) and enemy goals
        sides.Add(mainSide.gameObject.activeSelf ? mainSide : Instantiate(enemySidePrefab));
        for (int i = 1; i < numPlayers; i++)
        {
            var side = Instantiate(enemySidePrefab);
            side.gameObject.SetActive(true);
            sides.Add(side);
        }

        // set colors
        // var hueInc = 1f / players.Count;
        // var startHue = hueInc / 2f;
        // for (var i = 0; i < numPlayers; i++)
        //     players[i].SetColor(Color.HSVToRGB(startHue + (hueInc * i), 0.6f, 1f)); 

        // set goal positions
        SetGoalPositions(overTime: 0f);
    }
    
    private void SetGoalPositions(float overTime = 0f)
    {
        if (currentGoalTransition != null)
            StopCoroutine(currentGoalTransition);

        currentGoalTransition = StartCoroutine(TransitionPlayers(overTime));
    }

    private IEnumerator TransitionPlayers(float overTime = 0f)
    {
        // TODO: debug
        if (sides.Count == 2)
        {
            sides = new List<Side>() {sides[0], _rightSide, sides[1], _leftSide};
            _leftSide.gameObject.SetActive(true);
            _rightSide.gameObject.SetActive(true);
        }

        var startPositions = new Vector3[sides.Count];
        var endPositions = new Vector3[sides.Count];
        
        var startRotations = new Quaternion[sides.Count];
        var endRotations = new Quaternion[sides.Count];
        
        for (int i = 0; i < sides.Count; i++)
        {
            startPositions[i] = sides[i].gameObject.transform.position;
            startRotations[i] = sides[i].gameObject.transform.rotation;
        }
        
        // TODO: debug -- align polygon
        var closestIndex = -1;
        var minDistance = Mathf.Infinity;
        for (int i = 0; i < polygon.Points.Length; i++)
        {
            var point = polygon.Points[i];
            if (Vector3.Distance(startPositions[0], point) < minDistance)
                closestIndex = i;
        }

        var index = 0;
        for (int i = closestIndex; i < polygon.Points.Length; i++)
            endPositions[index++] = polygon.Points[i];

        for (int i = 0; i < closestIndex; i++)
            endPositions[index++] = polygon.Points[i];
        
        for (int i = 0; i < endPositions.Length; i++)
            endRotations[i] = quaternion.LookRotation(Vector3.forward, transform.position - endPositions[i]);
        
        float transition = 0f;
        float elapsedTime = 0f;
        
        while (transition < 1f)
        {
            var t = overTime > 0f ? elapsedTime / overTime : 1f;
            transition = Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);  // smooth stop
            
            // determine new goal positions first
            for (int i = 0; i < sides.Count; ++i)
            {
                var nextPos = Vector3.Lerp(startPositions[i], endPositions[i], transition);
                var nextRot = Quaternion.Lerp(startRotations[i], endRotations[i], transition);
                sides[i].gameObject.transform.position = nextPos;
                sides[i].gameObject.transform.rotation = nextRot;
            }

            // wait for the end of frame and yield
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        currentGoalTransition = null;
    }

    // public void GoalScored(GameObject side, Ball ball)
    public void GoalScored(Side side, Ball ball)
    {
        Debug.Log("Goal Scored!");
        
        
        // stop current transition
        if (currentGoalTransition != null)
            StopCoroutine(currentGoalTransition);
    
        // destroy the ball
        // ballManager.Remove(ball.gameObject);
    
        // destroy the goal
        if (sides.Remove(side))
        {
            Destroy(side.gameObject);
            OnNumPlayersChanged.Invoke(sides.Count);
        }
        else
        {
            Debug.Log("Side not in sides!");
        }
    
        // transition sectors
        switch (sides.Count)
        {
            case 1:
                return;
            case 2:
                polygon = new Polygon(4, sideLength);
                break;
            default:
                polygon = new Polygon(sides.Count, sideLength);
                break;
        }
        
        var time = Mathf.Lerp(startTransitionTime, endTransitionTime, 1.0f - ((float) sides.Count / numPlayers));
        SetGoalPositions(overTime: time);
    }

    public float GetDiameter()
    {
        return polygon.CircumRadius * 2f;
    }
}
