using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using Unity.Mathematics;
using UnityEngine;


public class ArenaManager : MonoBehaviour
{
    public int players = 5;
    public float radius = 2.5f;
    
    public void OnDrawGizmos()
    {
        // TODO: this should just draw a circle and an inscribed polygon with 5 sides
        
        var polygon = new Polygon(players, radius);

        Draw.LineGeometry = LineGeometry.Billboard;
        Draw.LineThickness = 0.05f;
        Draw.LineEndCaps = LineEndCap.Round;

        // set colors
        var hueInc = 1f / players;
        var startHue = hueInc / 2f;

        for (int i = 0; i < polygon.Positions.Length; i++)
        {
            var color = Color.HSVToRGB(startHue + (hueInc * i), 0.6f, 1f);
            var position = polygon.Positions[i];
            Draw.Line(position.left, position.right, color);
        }
    }


    // [SerializeField] float startTransitionTime = 0.5f;
    // [SerializeField] private float endTransitionTime = 3f;
    //
    // [SerializeField] int numPlayers;
    // [SerializeField] float radius;
    // public float Radius => radius;
    //
    // // [SerializeField] private Player enemyPlayerPrefab;
    //
    // // [SerializeField] private Boundary boundaryPrefab;
    // // private List<Boundary> boundaries;
    //
    // // public Player mainPlayer;
    // // private List<Player> players = new List<Player>();
    //
    // // [SerializeField] private BallManager ballManager;
    //
    // // [SerializeField] private Corner cornerPrefab;
    // // private List<Corner> corners;
    //
    // // private Polygon polygon;
    // private Coroutine currentGoalTransition = null;
    //
    // public static Arena current;
    //
    // public event Action OnTransitionStart;
    // public event Action OnTransitionEnd;
    //
    //
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     var gizmoPolygon = new Polygon(numPlayers, radius);
    //     foreach ((Vector3 from, Vector3 to) in gizmoPolygon.Positions)
    //     {
    //         Gizmos.DrawLine(from, to);
    //     }
    //
    //     var arenaOutline = Shapes.RegularPolygon.Instantiate();
    //
    //     
    //     
    //
    //     Draw.RegularPolygonGeometry();
    //
    // }
    //
    //
    // private void Start()
    // {
    //     current = this;
    //     
    //     // Application.targetFrameRate = 60;
    //
    //     Cursor.visible = false;
    //     
    //     // create polygon from which to find sector positions
    //     polygon = new Polygon(numPlayers, radius);
    //     
    //     // create goals
    //     players = new List<Player>();
    //     
    //     // add player (if defined) and enemy goals
    //     players.Add(mainPlayer.gameObject.activeSelf ? mainPlayer : Instantiate(enemyPlayerPrefab));
    //     for (int i = 1; i < numPlayers; i++)
    //         players.Add(Instantiate(enemyPlayerPrefab));
    //
    //     // set colors
    //     var hueInc = 1f / players.Count;
    //     var startHue = hueInc / 2f;
    //     for (var i = 0; i < numPlayers; i++)
    //         players[i].SetColor(Color.HSVToRGB(startHue + (hueInc * i), 0.6f, 1f)); 
    //     
    //     // listen to all goals
    //     foreach (Player p in players)
    //         p.OnPlayerEliminated += GoalScored;
    //     
    //     // create boundaries
    //     boundaries = new List<Boundary>();
    //     for (int i = 0; i < numPlayers; i++)
    //         boundaries.Add(Instantiate(boundaryPrefab));
    //     
    //     // set corner positions
    //     corners = new List<Corner>();
    //     for (int i = 0; i < numPlayers; i++)
    //         corners.Add(Instantiate(cornerPrefab));
    //     
    //     // set goal positions
    //     SetGoalPositions(overTime: 0f);
    // }
    //
    // // TODO: debug
    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.R))
    //     {
    //         // reset the current scene
    //         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    //     }
    // }
    //
    // private void SetGoalPositions(float overTime = 0f)
    // {
    //     if (currentGoalTransition != null)
    //         StopCoroutine(currentGoalTransition);
    //
    //     currentGoalTransition = StartCoroutine(TransitionPlayers(overTime));
    // }
    //
    // private IEnumerator TransitionPlayers(float overTime = 0f)
    // {
    //     // notify listeners
    //     OnTransitionStart?.Invoke();
    //     
    //     (Vector3 left, Vector3 right)[] startPositions = new (Vector3, Vector3)[players.Count];
    //     for (int i = 0; i < players.Count; i++)
    //     {
    //         startPositions[i] = (players[i].LeftBound, players[i].RightBound);
    //     }
    //
    //     float transition = 0f;
    //     float elapsedTime = 0f;
    //     
    //     while (transition < 1f)
    //     {
    //         var t = overTime > 0f ? elapsedTime / overTime : 1f;
    //         transition = Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);  // smooth stop
    //         
    //         // determine new goal positions first
    //         for (int i = 0; i < players.Count; ++i)
    //         {
    //             var leftPoint = Vector3.Lerp(startPositions[i].left, polygon.Positions[i].left, transition).normalized * radius;
    //             var rightPoint = Vector3.Lerp(startPositions[i].right, polygon.Positions[i].right, transition).normalized * radius;
    //             players[i].SetBounds(leftPoint, rightPoint);
    //             corners[i].transform.position = leftPoint;
    //         }
    //         
    //         // set boundaries
    //         for (int i = 0; i < boundaries.Count; i++)
    //         {
    //             boundaries[i].SetBounds(
    //                 players[i % players.Count].RightBound,
    //                 players[(i + 1) % players.Count].LeftBound);
    //         }
    //     
    //         // wait for the end of frame and yield
    //         elapsedTime += Time.deltaTime;
    //         yield return new WaitForEndOfFrame();
    //     }
    //     currentGoalTransition = null;
    //     
    //     // notify listeners
    //     OnTransitionEnd?.Invoke();
    // }
    //
    // public void GoalScored(Player player, Ball ball)
    // {
    //     // stop current transition
    //     if (currentGoalTransition != null)
    //         StopCoroutine(currentGoalTransition);
    //
    //     // destroy the ball
    //     ballManager.Remove(ball.gameObject);
    //
    //     // destroy the goal
    //     if (players.Remove(player))
    //     {
    //         // display score if player out
    //         if (player.CompareTag("Player") || players.Count <= 1)
    //             Canvas.instance.DisplayScore(playersRemaining: players.Count, playersTotal: numPlayers);
    //         
    //         Destroy(player.gameObject);
    //         
    //         var boundary = boundaries[0];
    //         boundaries.RemoveAt(0);
    //         Destroy(boundary.gameObject);
    //
    //         var corner = corners[0];
    //         corners.RemoveAt(0);
    //         Destroy(corner.gameObject);
    //     }
    //
    //     // transition sectors
    //     polygon = new Polygon(players.Count, radius);
    //     
    //     var time = Mathf.Lerp(startTransitionTime, endTransitionTime, 1.0f - ((float) players.Count / numPlayers));
    //     SetGoalPositions(overTime: time);
    // }
}

