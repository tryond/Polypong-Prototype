using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MiscUtil.Collections.Extensions;
using Unity.VisualScripting;
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

    private Dictionary<int, Vector2> _nodeTargetNormals;
    [CanBeNull] private Coroutine _transition;

    private Dictionary<int, int> _collapseNodes;
    private Dictionary<int, ArenaNode> _hashToNode;
    
    private void Awake()
    {
        _currentRadius = 0f;
        _targetRadius = 0f;
        
        _transition = null;
        
        _nodes = new List<ArenaNode>();
        _nodes.Add(Instantiate(arenaNodePrefab, transform.position, Quaternion.identity));
        
        _nodeTargetNormals = new Dictionary<int, Vector2>();
        _nodeTargetNormals[_nodes[0].GetHashCode()] = _nodes[0].transform.up;
        
        _collapseNodes = new Dictionary<int, int>();
        
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
            
            Debug.DrawLine(_nodes[i].transform.position, _nodes[(i + 1) % _nodes.Count].transform.position, Color.red, Time.deltaTime);
            
            Debug.DrawLine(
                _nodes[i].transform.position, 
                (_targetRadius * _nodeTargetNormals[nodeHash]) + (Vector2) transform.position,
                Color.green, 
                Time.deltaTime);
            
            Debug.DrawRay(_nodes[i].transform.position, _nodes[i].transform.up, Color.black, Time.deltaTime);
        }
        
        foreach (var entry in _collapseNodes)
            Debug.DrawLine(_hashToNode[entry.Key].transform.position, _hashToNode[entry.Value].transform.position, Color.yellow, Time.deltaTime);
        
        
        Debug.DrawLine(transform.position, (_currentRadius * transform.up) + transform.position, Color.blue, Time.deltaTime);
        Debug.DrawLine(transform.position, (_targetRadius * transform.right) + transform.position, Color.yellow, Time.deltaTime);
    }

    /*
     * TODO:
     *
     * Split on node selected during runtime.
     */

    public void Split()
    {
        var nodeIndex = Random.Range(0, _nodes.Count);
        var arenaNode = _nodes[nodeIndex];

        // split current node, and add to list
        var splitNode = Instantiate(arenaNode);
        _hashToNode[splitNode.GetHashCode()] = splitNode;
        _nodes.Insert(nodeIndex, splitNode);
        
        // set new targets, making this side flat
        _targetRadius = Polygon.GetRadius(_nodes.Count, sideLength);
        var targetNormals = Polygon.GetVertexNormals(_nodes.Count, arenaNode.transform.up, nodeIndex);
        for (int i = 0; i < _nodes.Count; i++)
            _nodeTargetNormals[_nodes[i].GetHashCode()] = targetNormals[i];

        if (_transition != null)
            StopCoroutine(_transition);
        
        _transition = StartCoroutine(MoveToTargets());
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
            var currentRadius = Mathf.Lerp(startRadius, _targetRadius, t);
            
            // update split node positions
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
                var node = _nodes[i];
                node.transform.up = Vector2.Lerp(baseNormals[node.GetHashCode()], _nodeTargetNormals[node.GetHashCode()], t);
                node.transform.position = (currentRadius * node.transform.up) + transform.position;
            }
            
            // update collapsed node positions
            foreach (KeyValuePair<int, int> entry in _collapseNodes)
            {
                var fromNode = _hashToNode[entry.Key];
                var toNode = _hashToNode[entry.Value];

                fromNode.transform.up = Vector2.Lerp(baseNormals[fromNode.GetHashCode()], _nodeTargetNormals[toNode.GetHashCode()], t);
                fromNode.transform.position = (currentRadius * fromNode.transform.up) + transform.position;
            }
        
            // wait for the end of frame and yield
            _currentRadius = currentRadius;
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _transition = null;
        
        /*
         * TODO:
         *
         * This will wait until the most recent coroutine ends to destroy collapsing nodes.
         *
         * If several collapses/splits happen simultaneously, this leaves collapsed nodes in
         * awkward positions.
         *
         * This would be addressed by stacking coroutines.
         */
        
        // clear out collapsed nodes
        foreach (var hash in _collapseNodes.Keys.ToList())
        {
            Debug.Log($"destroying node {hash}");
            
            var nodeToDestroy = _hashToNode[hash];
            _hashToNode.Remove(hash);
            _collapseNodes.Remove(hash);
            Destroy(nodeToDestroy.gameObject);
        }
    }

    private float SmoothStop(float timeElapsed)
    {
        var t = timeElapsed / transitionTime;
        return Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);
    }
    
    public void Collapse()
    {
        var nodeIndex = Random.Range(0, _nodes.Count);
        var arenaNode = _nodes[nodeIndex];

        /*
         * TODO:
         *
         * _nodes needs to keep collapseNode in list until destroyed.
         */
        
        // collapse current node, and remove from list
        var collapseNodeIndex = (nodeIndex + 1) % _nodes.Count;
        var collapseNode = _nodes[collapseNodeIndex];
        _nodes.RemoveAt(collapseNodeIndex);
        
        // set new targets, making this side flat
        _targetRadius = Polygon.GetRadius(_nodes.Count, sideLength);
        var targetNormals = Polygon.GetVertexNormals(_nodes.Count, arenaNode.transform.up, nodeIndex, false);
        for (int i = 0; i < _nodes.Count; i++)
            _nodeTargetNormals[_nodes[i].GetHashCode()] = targetNormals[i];

        _collapseNodes[collapseNode.GetHashCode()] = arenaNode.GetHashCode();

        if (_transition != null)
            StopCoroutine(_transition);
        
        _transition = StartCoroutine(MoveToTargets());
    }
}
