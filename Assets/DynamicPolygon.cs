using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class DynamicPolygon : MonoBehaviour
{
    [SerializeField] protected int numSides = 5;
    [SerializeField] protected float sideLength = 1f;
    [SerializeField] protected float transitionTime = 1f;
    [SerializeField] protected GameObject vertexPrefab;
    
    protected List<GameObject> vertexList;
    protected Dictionary<int, GameObject> vertexMap;
    protected HashSet<int> collapseVertices;
    protected Dictionary<int, Vector2> targetNormals;
    
    public float Radius { get; private set; }
    protected float targetRadius = 0f;
    
    [CanBeNull] protected Coroutine currentTransition;

    [SerializeField] UnityEvent OnTransitionStarted = new UnityEvent();
    [SerializeField] UnityEvent OnTransitionStopped = new UnityEvent();
    
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var polygon = new Polygon(numSides, sideLength);
        foreach ((Vector3 from, Vector3 to) in polygon.Positions)
            Gizmos.DrawLine(from, to);
    }
    
    protected virtual void Awake()
    {
        // create vertex collections
        vertexList = new List<GameObject>();
        vertexMap = new Dictionary<int, GameObject>();
        collapseVertices = new HashSet<int>();
        targetNormals = new Dictionary<int, Vector2>();
        
        // add to vertex list and map
        for (int i = 0; i < numSides; i++)
        {
            var vertex = Instantiate(vertexPrefab);
            vertex.SetActive(true);
            
            vertexList.Add(vertex);
            vertexMap[vertex.GetHashCode()] = vertex;
        }
        
        // set targets
        SetTargets(0, transform.up, false);
        
        // move to targets (instantly)
        StartCoroutine(MoveToTargets(0f));
    }

    // TODO: debug
    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        else if (Input.GetKeyDown(KeyCode.S))
            Split(Random.Range(0, vertexList.Count));
        
        else if (Input.GetKeyDown(KeyCode.C))
        {
            var activeVertices = vertexList.FindAll(
                v => !collapseVertices.Contains(v.GetHashCode()));

            if (activeVertices.Count <= 1)
                return;
            
            var randomActiveVertex = activeVertices[Random.Range(0, activeVertices.Count)];
            Collapse(vertexList.IndexOf(randomActiveVertex));
        }
    }
    
    // TODO: debug
    protected void FixedUpdate()
    {
        var outlineColor = Color.grey;
        var targetPositionColor = Color.yellow;
        var normalColor = Color.black;
        var currentRadiusColor = Color.green;
        var targetRadiusColor = Color.red;
        
        for (int i = 0; i < vertexList.Count; i++)
        {
            var nodeHash = vertexList[i].GetHashCode();
            
            // draw arena outline
            Debug.DrawLine(
                vertexList[i].transform.position,
                vertexList[(i + 1) % vertexList.Count].transform.position,
                outlineColor,
                Time.deltaTime);
            
            // draw path to target positions
            Debug.DrawLine(
                vertexList[i].transform.position, 
                (targetRadius * targetNormals[nodeHash]) + (Vector2) transform.position,
                targetPositionColor, 
                Time.deltaTime);
            
            // draw node normals
            Debug.DrawRay(
                vertexList[i].transform.position,
                vertexList[i].transform.up,
                normalColor,
                Time.deltaTime);
        }
        
        // draw current radius
        Debug.DrawLine(
            transform.position,
            (Radius * transform.up) + transform.position,
            currentRadiusColor,
            Time.deltaTime);
        
        // draw target radius
        Debug.DrawLine(transform.position,
            (targetRadius * transform.right) + transform.position,
            targetRadiusColor,
            Time.deltaTime);
    }

    public virtual bool Split(int vertexIndex)
    {
        var vertex = vertexList[vertexIndex];
        var nextVertex = vertexList[(vertexIndex + 1) % vertexList.Count];
        var vertexAdded = true;
        
        // if already collapsing, stop collapse
        if (collapseVertices.Contains(nextVertex.GetHashCode()))
        {
            collapseVertices.Remove(nextVertex.GetHashCode());
            vertexAdded = false;
        }
        
        // else, clone current vertex
        else
        {
            nextVertex = Instantiate(vertex);
            vertexMap[nextVertex.GetHashCode()] = nextVertex;
            vertexList.Insert(vertexIndex, nextVertex);
        }

        SetTargets(vertexIndex, vertex.transform.up);

        if (currentTransition != null)
            StopCoroutine(currentTransition);
        else
            OnTransitionStarted.Invoke();
        
        currentTransition = StartCoroutine(MoveToTargets(transitionTime));
        return vertexAdded;
    }

    public bool Collapse(GameObject vertex)
    {
        var vertexIndex = vertexList.IndexOf(vertex);
        return Collapse(vertexIndex);
    }
    
    public bool Collapse(int vertexIndex)
    {
        if (vertexList.Count <= 1)
            return false;

        Debug.Log($"Collapsing node at index {vertexIndex}");
        
        var vertex = vertexList[vertexIndex];
        
        if (collapseVertices.Contains(vertex.GetHashCode()))
            return false;
        
        collapseVertices.Add(vertex.GetHashCode());
        
        var nextVertexIndex = (vertexIndex + 1) % vertexList.Count;
        
        // if collapsing last node, adjust positioning
        if (nextVertexIndex < vertexIndex)
            vertexIndex--;
        
        SetTargets(vertexIndex, vertex.transform.up, false);

        if (currentTransition != null)
            StopCoroutine(currentTransition);
        else
            OnTransitionStarted.Invoke();
        
        currentTransition = StartCoroutine(MoveToTargets(transitionTime));
        return true;
    }
    
    protected void SetTargets(int baseIndex, Vector2 baseUp, bool outward = true)
    {
        var numActiveVertices = vertexList.Count - collapseVertices.Count;
        var normals = Polygon.GetVertexNormals(numActiveVertices, baseUp, 0, outward);

        var j = 0;
        for (var i = 0; i < vertexList.Count; i++)
        {
            var index = (baseIndex + i) % vertexList.Count;
            var vertex = vertexList[index];
            targetNormals[vertex.GetHashCode()] = normals[j];

            // only move to the next target if current vertex is not collapsing
            if (!collapseVertices.Contains(vertex.GetHashCode()))
                j = (j + 1) % normals.Length;
        }
        
        // set radius
        targetRadius = Polygon.GetRadius(numActiveVertices, sideLength);
    }
    
    protected IEnumerator MoveToTargets(float overTime)
    {
        var t = 0f;
        var elapsedTime = 0f;
        var startRadius = Radius;
        
        // set vertex starting normals
        var baseNormals = new Dictionary<int, Vector2>();
        foreach (KeyValuePair<int, GameObject> hashAndVertex in vertexMap)
            baseNormals[hashAndVertex.Key] = hashAndVertex.Value.transform.up;
        
        while (t < 1f)
        {
            t = SmoothStop(elapsedTime, overTime);
            Radius = Mathf.Lerp(startRadius, targetRadius, t);
            
            UpdateVertexPositions(baseNormals, targetNormals, t);

            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // remove collapsed vertices and reset transition
        RemoveCollapsedVertices();
        OnTransitionStopped.Invoke();
        currentTransition = null;
    }

    protected virtual void UpdateVertexPositions(Dictionary<int, Vector2> fromMap, Dictionary<int, Vector2> toMap, float t)
    {
        for (int i = 0; i < vertexList.Count; i++)
        {
            var vertex = vertexList[i];
            vertex.transform.up = Vector2.Lerp(fromMap[vertex.GetHashCode()], toMap[vertex.GetHashCode()], t);
            vertex.transform.position = (Radius * vertex.transform.up) + transform.position;
        }
    }
    
    protected virtual void RemoveCollapsedVertices()
    {
        // destroy collapsed vertices
        foreach (var vertexHash in collapseVertices.ToList())
        {
            var vertex = vertexMap[vertexHash];

            vertexList.Remove(vertex);
            vertexMap.Remove(vertexHash);
            collapseVertices.Remove(vertexHash);

            Debug.Log($"Removing vertex with hash {vertexHash}");
            
            Destroy(vertex.gameObject);
        }
    }

    protected static float SmoothStop(float elapsedTime, float totalTime)
    {
        if (totalTime <= 0f)
            return 1f;
        
        var t = elapsedTime / totalTime;
        return Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);
    }
}
