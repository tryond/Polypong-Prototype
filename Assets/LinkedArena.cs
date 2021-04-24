using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Dictionary<int, ArenaNode> _hashToNode;
    
    private float _currentRadius;
    private float _targetRadius;
    
    private Dictionary<int, Vector2> _nodeTargetNormals;
    [CanBeNull] private Coroutine _transition;

    // for debug purposes only
    private Vector2 lastBaseUp = Vector2.up;
    
    private void Awake()
    {
        _currentRadius = 0f;
        _targetRadius = 0f;
        
        _transition = null;
        
        _nodes = new List<ArenaNode>();
        _nodes.Add(Instantiate(arenaNodePrefab, transform.position, Quaternion.identity));
        
        _nodeTargetNormals = new Dictionary<int, Vector2>();
        _nodeTargetNormals[_nodes[0].GetHashCode()] = _nodes[0].transform.up;
        
        _hashToNode = new Dictionary<int, ArenaNode>();
        _hashToNode[_nodes[0].GetHashCode()] = _nodes[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        else if (Input.GetKeyDown(KeyCode.S))
            Split();
        
        else if (Input.GetKeyDown(KeyCode.C))
            Collapse();
    }
    
    private void FixedUpdate()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            var nodeHash = _nodes[i].GetHashCode();
            
            // draw arena outline
            Debug.DrawLine(_nodes[i].transform.position, _nodes[(i + 1) % _nodes.Count].transform.position, Color.red, Time.deltaTime);
            
            // draw path to target positions
            Debug.DrawLine(
                _nodes[i].transform.position, 
                (_targetRadius * _nodeTargetNormals[nodeHash]) + (Vector2) transform.position,
                Color.green, 
                Time.deltaTime);
            
            // draw node normals
            Debug.DrawRay(_nodes[i].transform.position, _nodes[i].transform.up, Color.black, Time.deltaTime);
        }
        
        Debug.DrawLine(transform.position, (_currentRadius * transform.up) + transform.position, Color.blue, Time.deltaTime);
        Debug.DrawLine(transform.position, (_targetRadius * transform.right) + transform.position, Color.yellow, Time.deltaTime);
        
        // draw last base up
        Debug.DrawLine(transform.position,  (2f * _targetRadius * lastBaseUp) + (Vector2) transform.position, Color.magenta, Time.deltaTime);
    }

    public void Split()
    {
        var nodeIndex = Random.Range(0, _nodes.Count);
        var currentNode = _nodes[nodeIndex];
        Debug.Log($"Splitting node {nodeIndex} of {_nodes.Count - 1}");    // TODO: remove
        
        var nextNode = _nodes[(nodeIndex + 1) % _nodes.Count];
        if (!nextNode.active)
            nextNode.active = true;
        else
        {
            nextNode = Instantiate(currentNode);
            nextNode.active = true;
            _hashToNode[nextNode.GetHashCode()] = nextNode;
            _nodes.Insert(nodeIndex, nextNode);
        }

        SetTargets(nodeIndex, currentNode.transform.up, true);

        if (_transition != null)
            StopCoroutine(_transition);
        _transition = StartCoroutine(MoveToTargets());
    }

    public void Collapse()
    {
        var nodeIndex = Random.Range(0, _nodes.Count);
        var currentNode = _nodes[nodeIndex];
        
        // collapse current node, and remove from list
        var nextNodeIndex = (nodeIndex + 1) % _nodes.Count;
        var nextNode = _nodes[nextNodeIndex];

        if (!nextNode.active)
            return;
        nextNode.active = false;
        
        Debug.Log($"Collapsing node {nextNodeIndex} -> {nodeIndex}");    // TODO: remove

        // if collapsing last node, adjust positioning
        if (nextNodeIndex < nodeIndex)
            nodeIndex--;
        
        SetTargets(nodeIndex, currentNode.transform.up, false);

        if (_transition != null)
            StopCoroutine(_transition);
        _transition = StartCoroutine(MoveToTargets());
    }

    private void SetTargets(int baseIndex, Vector2 baseUp, bool outward = true)
    {
        var numActiveNodes = _nodes.Count(node => node.active);

        Debug.Log($"Setting targets for {numActiveNodes} active nodes");
        
        // TODO: debug
        lastBaseUp = baseUp;
        
        // TODO: the baseIndex could be 12, but there could really be like 3 inactive nodes behind it
        // set normals
        var targetNormals = Polygon.GetVertexNormals(numActiveNodes, baseUp, 0, outward);

        var j = 0;
        for (var i = 0; i < _nodes.Count; i++)
        {
            var index = (baseIndex + i) % _nodes.Count;
            var node = _nodes[index];
            _nodeTargetNormals[node.GetHashCode()] = targetNormals[j];

            if (node.active)
                j = (j + 1) % targetNormals.Length;
        }
        
        // set radius
        _targetRadius = Polygon.GetRadius(numActiveNodes, sideLength);
    }
    
    public IEnumerator MoveToTargets()
    {
        float t = 0f;
        float timeElapsed = 0f;

        var baseNormals = new Dictionary<int, Vector2>();
        foreach (KeyValuePair<int, ArenaNode> entry in _hashToNode)
            baseNormals[entry.Key] = entry.Value.transform.up;
        
        var startRadius = _currentRadius;
        while (t < 1f)
        {
            t = SmoothStop(timeElapsed);
            
            // update radius
            var radius = Mathf.Lerp(startRadius, _targetRadius, t);
            
            // update node positions
            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                node.transform.up = Vector2.Lerp(baseNormals[node.GetHashCode()], _nodeTargetNormals[node.GetHashCode()], t);
                node.transform.position = (radius * node.transform.up) + transform.position;
                
                // TODO: if node collapsing and within threshold distance, remove (!)
                // if (!node.active && Vector2.Distance(node, _nodes[(i - 1) % _nodes.Count]))
            }

            // wait for the end of frame and yield
            _currentRadius = radius;
            timeElapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
            
            // fix camera to base node
            // if (_nodes.Count >= 2)
            //     cam.transform.up = -(_nodes[0].transform.up + _nodes[1].transform.up).normalized;
        }
        _transition = null;

        // clear out collapsed nodes
        foreach (var node in _nodes.ToList())
            if (!node.active)
            {
                _nodes.Remove(node);
                _hashToNode.Remove(node.GetHashCode());
                Destroy(node.gameObject);
            }
    }

    private float SmoothStop(float timeElapsed)
    {
        var t = timeElapsed / transitionTime;
        return Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);
    }
}
