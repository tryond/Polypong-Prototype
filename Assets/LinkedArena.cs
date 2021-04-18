using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LinkedArena : MonoBehaviour
{
    public float sideLength = 1f;
    public float transitionTime = 1f;
    public ArenaNode arenaNodePrefab;
    
    private List<ArenaNode> _nodes;
    
    private float _currentRadius;
    private float _targetRadius;
    private Vector2[] _targetNormals;
    
    [CanBeNull] private Coroutine _transition;
    
    private void Awake()
    {
        _currentRadius = 0f;
        _targetRadius = 0f;
        
        _targetNormals = new [] { (Vector2) transform.up };
        _transition = null;
        
        _nodes = new List<ArenaNode>();
        _nodes.Add(Instantiate(arenaNodePrefab, transform.position, Quaternion.identity));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void FixedUpdate()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            Debug.DrawLine(_nodes[i].transform.position, _nodes[(i + 1) % _nodes.Count].transform.position, Color.red, Time.deltaTime);
            
            Debug.DrawLine(
                _nodes[i].transform.position, 
                (_targetRadius * _targetNormals[i]) + (Vector2) transform.position,
                Color.green, 
                Time.deltaTime);
            
            Debug.DrawRay(_nodes[i].transform.position, _nodes[i].transform.up, Color.black, Time.deltaTime);
            
        }
        Debug.DrawLine(transform.position, (_currentRadius * transform.up) + transform.position, Color.blue, Time.deltaTime);
        Debug.DrawLine(transform.position, (_targetRadius * transform.right) + transform.position, Color.yellow, Time.deltaTime);
    }

    
    public void Split()
    {
        var nodeIndex = Random.Range(0, _nodes.Count);
        var arenaNode = _nodes[nodeIndex];

        // split current node, and add to list
        var splitNode = Instantiate(arenaNode);
        _nodes.Insert(nodeIndex, splitNode);
        
        // set new targets, making this side flat
        _targetRadius = Polygon.GetRadius(_nodes.Count, sideLength);
        _targetNormals = Polygon.GetVertexNormals(_nodes.Count, arenaNode.transform.up, nodeIndex);

        if (_transition != null)
            StopCoroutine(_transition);
        
        _transition = StartCoroutine(SplitRoutine());
    }

    public IEnumerator SplitRoutine()
    {
        float t = 0f;
        float timeElapsed = 0f;

        Vector2[] startNormals = new Vector2[_nodes.Count];
        for (int i = 0; i < _nodes.Count; i++)
            startNormals[i] = _nodes[i].transform.up;

        var startRadius = _currentRadius;
        while (t < 1f)
        {
            t = SmoothStop(timeElapsed);
            var currentRadius = Mathf.Lerp(startRadius, _targetRadius, t);
            
            // update node transforms
            for (int i = 0; i < _nodes.Count; i++)
            {
                /*
                 * TODO:
                 *
                 * Because this directly sets the node's up vector, any concurrent
                 * coroutine calls will overwrite each other.
                 *
                 * This is manageable for now, but clunky. A proper solution would
                 * combine rotations from all concurrent coroutines and apply
                 * those rotations on FixedUpdate.
                 */
                
                _nodes[i].transform.up = Vector2.Lerp(startNormals[i], _targetNormals[i], t);
                _nodes[i].transform.position = (currentRadius * _nodes[i].transform.up) + transform.position;
            }
        
            // wait for the end of frame and yield
            _currentRadius = currentRadius;
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _transition = null;
    }

    private float SmoothStop(float timeElapsed)
    {
        var t = timeElapsed / transitionTime;
        return Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);
    }
}
