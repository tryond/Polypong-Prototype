using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class DynamicPolygon : MonoBehaviour
{
    [SerializeField] private int numSides = 5;
    [SerializeField] private float sideLength = 1f;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private GameObject vertexPrefab;
    
    private List<GameObject> verticesList;
    private Dictionary<int, GameObject> verticesMap;
    private HashSet<int> collapseVertices;
    private Dictionary<int, Vector2> targetNormals;
    
    private float currentRadius = 0f;
    private float targetRadius = 0f;
    
    [CanBeNull] private Coroutine currentTransition;

    // for debug purposes only
    private Vector2 lastBaseUp = Vector2.up;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var polygon = new Polygon(numSides, sideLength);
        foreach ((Vector3 from, Vector3 to) in polygon.Positions)
            Gizmos.DrawLine(from, to);
    }
    
    private void Awake()
    {
        // create vertex collections
        verticesList = new List<GameObject>();
        verticesMap = new Dictionary<int, GameObject>();
        collapseVertices = new HashSet<int>();
        targetNormals = new Dictionary<int, Vector2>();
        
        // add to vertex list and map
        for (int i = 0; i < numSides; i++)
        {
            var vertex = Instantiate(vertexPrefab);
            vertex.SetActive(true);
            
            verticesList.Add(vertex);
            verticesMap[vertex.GetHashCode()] = vertex;
        }
        
        // set targets
        SetTargets(0, transform.up, false);
        
        // move to targets (instantly)
        StartCoroutine(MoveToTargets(0f));
    }
    
    public DynamicPolygon(float sideLength, int numSides, float transitionTime, GameObject vertexPrefab)
    {
        this.numSides = numSides;
        this.sideLength = sideLength;
        this.transitionTime = transitionTime;
        this.vertexPrefab = vertexPrefab;
    }

    // TODO: debug
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        else if (Input.GetKeyDown(KeyCode.S))
            Split(Random.Range(0, verticesList.Count));
        
        else if (Input.GetKeyDown(KeyCode.C))
        {
            var activeVertices = verticesList.FindAll(
                v => !collapseVertices.Contains(v.GetHashCode()));

            if (activeVertices.Count <= 1)
                return;
            
            var randomActiveVertex = activeVertices[Random.Range(0, activeVertices.Count)];
            Collapse(verticesList.IndexOf(randomActiveVertex));
        }
    }
    
    // TODO: debug
    private void FixedUpdate()
    {
        for (int i = 0; i < verticesList.Count; i++)
        {
            var nodeHash = verticesList[i].GetHashCode();
            
            // draw arena outline
            Debug.DrawLine(verticesList[i].transform.position, verticesList[(i + 1) % verticesList.Count].transform.position, Color.red, Time.deltaTime);
            
            // draw path to target positions
            Debug.DrawLine(
                verticesList[i].transform.position, 
                (targetRadius * targetNormals[nodeHash]) + (Vector2) transform.position,
                Color.green, 
                Time.deltaTime);
            
            // draw node normals
            Debug.DrawRay(verticesList[i].transform.position, verticesList[i].transform.up, Color.black, Time.deltaTime);
        }
        
        Debug.DrawLine(transform.position, (currentRadius * transform.up) + transform.position, Color.blue, Time.deltaTime);
        Debug.DrawLine(transform.position, (targetRadius * transform.right) + transform.position, Color.yellow, Time.deltaTime);
        
        // draw last base up
        Debug.DrawLine(transform.position,  (2f * targetRadius * lastBaseUp) + (Vector2) transform.position, Color.magenta, Time.deltaTime);
    }

    public void Split(int vertexIndex)
    {
        var vertex = verticesList[vertexIndex];
        var nextVertex = verticesList[(vertexIndex + 1) % verticesList.Count];

        // if already collapsing, stop collapse
        if (collapseVertices.Contains(nextVertex.GetHashCode()))
            collapseVertices.Remove(nextVertex.GetHashCode());
        
        // else, clone current vertex
        else
        {
            nextVertex = Instantiate(vertex);
            verticesMap[nextVertex.GetHashCode()] = nextVertex;
            verticesList.Insert(vertexIndex, nextVertex);
        }

        SetTargets(vertexIndex, vertex.transform.up);

        if (currentTransition != null)
            StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(MoveToTargets(transitionTime));
    }

    public void Collapse(int vertexIndex)
    {
        if (verticesList.Count <= 1)
            return;
        
        var vertex = verticesList[vertexIndex];
        var nextVertexIndex = (vertexIndex + 1) % verticesList.Count;
        var nextVertex = verticesList[nextVertexIndex];

        // if already collapsing, return
        if (collapseVertices.Contains(nextVertex.GetHashCode()))
            return;

        collapseVertices.Add(nextVertex.GetHashCode());
        
        // if collapsing last node, adjust positioning
        if (nextVertexIndex < vertexIndex)
            vertexIndex--;
        
        SetTargets(vertexIndex, vertex.transform.up, false);

        if (currentTransition != null)
            StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(MoveToTargets(transitionTime));
    }
    
    private void SetTargets(int baseIndex, Vector2 baseUp, bool outward = true)
    {
        var numActiveVertices = verticesList.Count - collapseVertices.Count;
        var normals = Polygon.GetVertexNormals(numActiveVertices, baseUp, 0, outward);

        var j = 0;
        for (var i = 0; i < verticesList.Count; i++)
        {
            var index = (baseIndex + i) % verticesList.Count;
            var vertex = verticesList[index];
            targetNormals[vertex.GetHashCode()] = normals[j];

            // only move to the next target if current vertex is not collapsing
            if (!collapseVertices.Contains(vertex.GetHashCode()))
                j = (j + 1) % normals.Length;
        }
        
        // set radius
        targetRadius = Polygon.GetRadius(numActiveVertices, sideLength);
    }
    
    public IEnumerator MoveToTargets(float overTime)
    {
        var t = 0f;
        var elapsedTime = 0f;
        var startRadius = currentRadius;
        
        // set vertex starting normals
        var baseNormals = new Dictionary<int, Vector2>();
        foreach (KeyValuePair<int, GameObject> hashAndVertex in verticesMap)
            baseNormals[hashAndVertex.Key] = hashAndVertex.Value.transform.up;
        
        while (t < 1f)
        {
            t = SmoothStop(elapsedTime, overTime);
            currentRadius = Mathf.Lerp(startRadius, targetRadius, t);
            
            // update vertex normals
            for (int i = 0; i < verticesList.Count; i++)
            {
                var vertex = verticesList[i];
                vertex.transform.up = Vector2.Lerp(baseNormals[vertex.GetHashCode()], targetNormals[vertex.GetHashCode()], t);
                vertex.transform.position = (currentRadius * vertex.transform.up) + transform.position;
            }

            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // destroy collapsed vertices
        foreach (var vertexHash in collapseVertices.ToList())
        {
            var vertex = verticesMap[vertexHash];

            verticesList.Remove(vertex);
            verticesMap.Remove(vertexHash);
            collapseVertices.Remove(vertexHash);
            
            Destroy(vertex.gameObject);
        }

        // reset transition
        currentTransition = null;
    }

    private static float SmoothStop(float elapsedTime, float totalTime)
    {
        if (totalTime <= 0f)
            return 1f;
        
        var t = elapsedTime / totalTime;
        return Mathf.Clamp(1 - (1 - t) * (1 - t) * (1 - t), 0f, 1f);
    }
}
