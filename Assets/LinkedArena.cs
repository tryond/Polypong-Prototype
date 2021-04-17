using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LinkedArena : MonoBehaviour
{
    // public float sideLength = 1f;
    public float radius = 1f;
    public float transitionTime = 1000f;
    public ArenaNode arenaNodePrefab;
    
    private List<ArenaNode> _nodes;
    private Dictionary<int, float> _transitions;
    private int _lastTransitionIndex;
    private float _maxAngle;
    
    private void Awake()
    {
        // TODO: remove
        Application.targetFrameRate = 60;
        
        _nodes = new List<ArenaNode>();
        _nodes.Add(Instantiate(arenaNodePrefab));
        _transitions = new Dictionary<int, float>();
        _lastTransitionIndex = 0;
    }

    private void FixedUpdate()
    {
        if (_transitions.Count <= 0)
            return;

        // TODO: this is expensive...
        var spacing = CalculateSpacing();
        
        for (int i = 0; i < _nodes.Count; i++)
        {
            var index = (_lastTransitionIndex + i) % _nodes.Count;

            var currentNode = _nodes[index];
            var nextNode = _nodes[(index + 1) % _nodes.Count];
            
            // set next node's position and rotation
            nextNode.transform.up = Quaternion.Euler(0f, 0f, spacing[index]) * currentNode.transform.up;
            nextNode.transform.position = (radius * nextNode.transform.up) + transform.position;
        }

        foreach (var key in _transitions.Keys)
            _transitions[key] += Time.deltaTime;
    }

    private float[] CalculateSpacing()
    {
        var numNodes = _nodes.Count;
        var spacing = new float[numNodes];
        var totalTransitionSpacing = 0f;
        var transitionKeysToRemove = new List<int>();
        
        // first calculate transition spacing
        foreach(var transition in _transitions)
        {
            var t = transition.Value / transitionTime;
            
            // TODO: debug
            Debug.Log($"t = {t}");
            
            
            t = Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);  // smooth stop
            
            var transitionSpacing = _maxAngle * t;

            spacing[transition.Key] = transitionSpacing;
            totalTransitionSpacing += transitionSpacing;

            // remove transition if completed
            if (t >= 1f)
            {
                Debug.Log($"Remove transition {transition.Key}");
                transitionKeysToRemove.Add(transition.Key);
            }
        }

        // set non-transition spacing
        var staticSpacing = (360f - totalTransitionSpacing) / (numNodes - _transitions.Count);
        for (int i = 0; i < numNodes; i++)
        {
            if (_transitions.Keys.Contains(i))
                continue;

            spacing[i] = staticSpacing;
        }
        
        // remove transition keys
        foreach (var key in transitionKeysToRemove)
            _transitions.Remove(key);

        return spacing;
    }
    
    public void Expand(int nodeIndex)
    {
        nodeIndex = Random.Range(0, _nodes.Count); // TODO: debug
        Debug.Log($"Expand {nodeIndex}");
        
        if (_nodes.Count <= nodeIndex)
            return;

        _lastTransitionIndex = nodeIndex;
        
        // clone current node
        var currentNode = _nodes[nodeIndex];
        var childNode = Instantiate(currentNode);
        _nodes.Insert(nodeIndex + 1, childNode);

        // add transition to dictionary
        _maxAngle = 360f / _nodes.Count;
        _transitions[nodeIndex] = 0f;
    }
    
    // TODO
    public void Collapse(int nodeIndex)
    {
        throw new NotImplementedException();
    }
}
