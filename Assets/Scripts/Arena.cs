using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Arena : MonoBehaviour
{
    [SerializeField] float startTransitionTime = 0.5f;
    [SerializeField] private float endTransitionTime = 3f;
    
    [SerializeField] int numPlayers;
    [SerializeField] float sideLength;
    
    [SerializeField] private GameObject enemySidePrefab;

    public BallManager bm;
    
    // [SerializeField] private GameObject goalPrefab;
    private List<GameObject> goals;
    
    public GameObject mainSide;
    private List<GameObject> sides = new List<GameObject>();
    
    // [SerializeField] private BallManager ballManager;

    // [SerializeField] private GameObject postPrefab;
    // private List<GameObject> posts;
    
    private Polygon polygon;
    private Coroutine currentGoalTransition = null;

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
        // create polygon from which to find sector positions
        polygon = new Polygon(numPlayers, sideLength);
        
        // create goals
        sides = new List<GameObject>();
        
        // add player (if defined) and enemy goals
        sides.Add(mainSide.gameObject.activeSelf ? mainSide : Instantiate(enemySidePrefab));
        for (int i = 1; i < numPlayers; i++)
        {
            var side = Instantiate(enemySidePrefab);
            side.SetActive(true);
            sides.Add(side);
        }
            

        // set colors
        // var hueInc = 1f / players.Count;
        // var startHue = hueInc / 2f;
        // for (var i = 0; i < numPlayers; i++)
        //     players[i].SetColor(Color.HSVToRGB(startHue + (hueInc * i), 0.6f, 1f)); 
        
        // listen to all goals
        // foreach (Player p in players)
        //     p.OnPlayerEliminated += GoalScored;
        
        // create boundaries
        // boundaries = new List<Boundary>();
        // for (int i = 0; i < numPlayers; i++)
        //     boundaries.Add(Instantiate(boundaryPrefab));
        
        // set corner positions
        // posts = new List<GameObject>();
        // for (int i = 0; i < numPlayers; i++)
        //     posts.Add(Instantiate(postPrefab));
        
        // set goal positions
        SetGoalPositions(overTime: 0f);
    }

    // TODO: debug
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // reset the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void SetGoalPositions(float overTime = 0f)
    {
        if (currentGoalTransition != null)
            StopCoroutine(currentGoalTransition);

        currentGoalTransition = StartCoroutine(TransitionPlayers(overTime));
    }

    private IEnumerator TransitionPlayers(float overTime = 0f)
    {
        // notify listeners
        // OnTransitionStart?.Invoke();
        
        var startPositions = new Vector3[sides.Count];
        var startRotations = new Quaternion[sides.Count];
        var endRotations = new Quaternion[sides.Count];
        for (int i = 0; i < sides.Count; i++)
        {
            startPositions[i] = sides[i].gameObject.transform.position;
            startRotations[i] = sides[i].gameObject.transform.rotation;
            endRotations[i] = quaternion.LookRotation(Vector3.forward, transform.position - polygon.Points[i]);
        }

        float transition = 0f;
        float elapsedTime = 0f;
        
        while (transition < 1f)
        {
            var t = overTime > 0f ? elapsedTime / overTime : 1f;
            transition = Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);  // smooth stop
            
            // determine new goal positions first
            for (int i = 0; i < sides.Count; ++i)
            {
                var nextPos = Vector3.Lerp(startPositions[i], polygon.Points[i], transition);
                var nextRot = Quaternion.Lerp(startRotations[i], endRotations[i], transition);
                // players[i].SetBounds(leftPoint, rightPoint);
                // corners[i].transform.position = leftPoint;
                sides[i].gameObject.transform.position = nextPos;
                // sides[i].gameObject.transform.rotation = quaternion.LookRotation(Vector3.forward, transform.position - nextPos);
                sides[i].gameObject.transform.rotation = nextRot;
            }
            
            // set boundaries
            // for (int i = 0; i < boundaries.Count; i++)
            // {
            //     boundaries[i].SetBounds(
            //         players[i % players.Count].RightBound,
            //         players[(i + 1) % players.Count].LeftBound);
            // }
        
            // wait for the end of frame and yield
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        currentGoalTransition = null;
        
        // notify listeners
        // OnTransitionEnd?.Invoke();
    }

    // public void GoalScored(GameObject side, Ball ball)
    public void GoalScored(GameObject side)
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
            // display score if player out
            // if (player.CompareTag("Player") || players.Count <= 1)
            //     Canvas.instance.DisplayScore(playersRemaining: players.Count, playersTotal: numPlayers);
            
            Destroy(side.gameObject);
            
            // var boundary = boundaries[0];
            // boundaries.RemoveAt(0);
            // Destroy(boundary.gameObject);
    
            // var corner = corners[0];
            // corners.RemoveAt(0);
            // Destroy(corner.gameObject);
        }
        else
        {
            Debug.Log("Side not in sides!");
        }
    
        // transition sectors
        polygon = new Polygon(sides.Count, sideLength);
        
        var time = Mathf.Lerp(startTransitionTime, endTransitionTime, 1.0f - ((float) sides.Count / numPlayers));
        SetGoalPositions(overTime: time);
    }

    public float GetDiameter()
    {
        return polygon.CircumRadius * 2f;
    }
    
}
