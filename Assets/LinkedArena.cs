using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MiscUtil.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class LinkedArena : MonoBehaviour
{
    public float sideLength = 1f;
    public float transitionTime = 1000f;
    public ArenaNode arenaNodePrefab;
    
    private List<ArenaNode> _nodes;
    private float _currentRadius;
    private float _targetRadius;
    private Vector2[] _targetNormals;
    [CanBeNull] private Coroutine _transition;
    
    
    private void Awake()
    {
        // TODO: remove
        Application.targetFrameRate = 60;

        _currentRadius = 0f;
        _targetRadius = 0f;
        
        _targetNormals = new [] { (Vector2) transform.up };
        _transition = null;
        
        _nodes = new List<ArenaNode>();
        _nodes.Add(Instantiate(arenaNodePrefab, transform.position, Quaternion.identity));
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
            
            Debug.DrawLine(transform.position, (_currentRadius * transform.up) + transform.position, Color.blue, Time.deltaTime);
            Debug.DrawLine(transform.position, (_targetRadius * transform.right) + transform.position, Color.yellow, Time.deltaTime);
        }
    }

    
    public void Split()
    {
        // // TODO: don't hardcode
        // var arenaNode = _nodes[0];
        //
        // // find node index of node to split
        // var nodeIndex = _nodes.IndexOf(arenaNode);

        var nodeIndex = Random.Range(0, _nodes.Count);
        var arenaNode = _nodes[nodeIndex];

        // split current node, and add to list
        var splitNode = Instantiate(arenaNode);
        _nodes.Insert(nodeIndex, splitNode);
        
        // set new targets, making this side flat
        _targetRadius = Polygon.GetRadius(_nodes.Count, sideLength);
        
        var rotatedTargetNormals = Polygon.GetVertexNormals(_nodes.Count, arenaNode.transform.up);

        _targetNormals = new Vector2[rotatedTargetNormals.Length];
        for (int i = 0; i < rotatedTargetNormals.Length; i++)
            _targetNormals[(nodeIndex + i) % _targetNormals.Length] = rotatedTargetNormals[i];
        
        if (_transition != null)
            StopCoroutine(_transition);
        
        _transition = StartCoroutine(SplitRoutine());
    }
    

    public IEnumerator SplitRoutine()
    {
        float transition = 0f;
        float elapsedTime = 0f;

        Vector2[] startNormals = new Vector2[_nodes.Count];
        for (int i = 0; i < _nodes.Count; i++)
            startNormals[i] = _nodes[i].transform.up;

        var startRadius = _currentRadius;
        while (transition < 1f)
        {
            var t = elapsedTime / transitionTime;
            // transition = Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);  // smooth stop
            transition = Mathf.Clamp(t, 0f, 1f);
            
            var currentRadius = Mathf.Lerp(startRadius, _targetRadius, t);
            
            // TODO: this should move based on rotation and radius -- not position !!!
            for (int i = 0; i < _nodes.Count; i++)
            {
                // TODO: this normal LERP is direction agnostic
                // TODO: it should always go in the direction that makes it between its parents
                _nodes[i].transform.up = Vector2.Lerp(startNormals[i], _targetNormals[i], t);

                _nodes[i].transform.position = (currentRadius * _nodes[i].transform.up) + transform.position;
            }
        
            // wait for the end of frame and yield
            _currentRadius = currentRadius;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _transition = null;
    }
}
