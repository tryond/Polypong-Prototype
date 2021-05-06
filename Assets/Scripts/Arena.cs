using System;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Arena : DynamicPolygon
{
    [SerializeField] private Side sidePrefab;

    [SerializeField] private Side playerSide;
    [SerializeField] private Side enemySidePrefab;
    
    private List<Side> sideList;
    protected Dictionary<int, Side> sideMap;
    
    protected Dictionary<int, int> vertexToSide;
    protected Dictionary<int, int> sideToVertex;
    
    public UnityEvent<int> OnNumSidesChanged = new UnityEvent<int>();

    protected override void Awake()
    {
        sideList = new List<Side>();
        sideMap = new Dictionary<int, Side>();
        
        vertexToSide = new Dictionary<int, int>();
        sideToVertex = new Dictionary<int, int>();
        
        base.Awake();
    }

    protected void Start()
    {
        for (int i = 0; i < vertexList.Count; i++)
        {
            var side = i == 0 ? playerSide : Instantiate(enemySidePrefab);
            var vertex = vertexList[i];
            
            sideList.Add(side);
            sideMap[side.GetHashCode()] = side;
            
            vertexToSide[vertex.GetHashCode()] = side.GetHashCode();
            sideToVertex[side.GetHashCode()] = vertex.GetHashCode();
            
            side.gameObject.SetActive(true);
        }
        UpdateSidePositions();
    }
    
    private void UpdateSidePositions()
    {
        // if (sideList.Count != vertexList.Count)
        //     throw new Exception("number of sides and vertices must be the same");
        
        for (int i = 0; i < sideList.Count; i++)
        {
            var leftVertex = vertexList[i];
            var rightVertex = vertexList[(i + 1) % vertexList.Count];

            var leftPos = leftVertex.transform.position;
            var rightPos = rightVertex.transform.position;

            var side = sideList[i];
            var sideTransform = side.transform;

            sideTransform.position = leftPos + (rightPos - leftPos) / 2f;
            sideTransform.up = (transform.position - sideTransform.position).normalized;
            
            side.SetLength(Vector3.Distance(leftPos, rightPos));

            var startColor = GetColorFromNormal(leftVertex.transform.up);
            var endColor = GetColorFromNormal(rightVertex.transform.up);
            side.SetColors(startColor, endColor);
        }
    }

    public void SplitRandom()
    {
        var randomIndex = Random.Range(0, vertexList.Count);
        Split(randomIndex);
    }

    public void CollapseRandom()
    {
        var randomIndex = Random.Range(0, vertexList.Count);
        Collapse(randomIndex);
    }

    public void DestroySide(Side side, Ball ball)
    {
        side.SetPaddle(null);
        
        var paddleCount = sideList.Count(s => s.paddle);
        OnNumSidesChanged.Invoke(paddleCount);
        
        // TODO: this whole section needs work...
        // consider revisiting the API, this is getting messy...
        
        // TODO: check if only two active sides remaining
        if (paddleCount == 2)
        {
            var nextIndex = (sideList.IndexOf(side) + 2) % sideList.Count;
            Split(nextIndex);    // TODO: this should split the opposite side
        }
        else
        {
            var vertexHash = sideToVertex[side.GetHashCode()];
            var vertex = vertexMap[vertexHash];
            Collapse(vertex);
        }
    }
    
    
    public override bool Split(int vertexIndex)
    {
        var vertexAdded = base.Split(vertexIndex);
        if (!vertexAdded)
            return false;
        
        // instantiate off-camera
        var side = Instantiate(sidePrefab, new Vector3(1000f, 1000f, 0f), Quaternion.identity);
        sideList.Insert(vertexIndex, side);
        sideMap[side.GetHashCode()] = side;
        
        vertexToSide[vertexList[vertexIndex].GetHashCode()] = side.GetHashCode();
        sideToVertex[side.GetHashCode()] = vertexList[vertexIndex].GetHashCode();
        
        return true;
    }

    protected override void UpdateVertexPositions(Dictionary<int, Vector2> fromMap, Dictionary<int, Vector2> toMap, float t)
    {
        base.UpdateVertexPositions(fromMap, toMap, t);
        UpdateSidePositions();
    }

    protected override void RemoveCollapsedVertices()
    {
        // destroy collapsed sides
        foreach (var vertexHash in collapseVertices.ToList())
        {
            var sideHash = vertexToSide[vertexHash];
            var side = sideMap[sideHash];

            sideList.Remove(side);
            sideMap.Remove(sideHash);
            
            vertexToSide.Remove(vertexHash);
            sideToVertex.Remove(sideHash);
            
            Destroy(side.gameObject);
        }
        base.RemoveCollapsedVertices();
    }

    private Color GetColorFromNormal(Vector3 normal)
    {
        var angle = Vector3.SignedAngle(Vector3.right, normal, Vector3.forward);
        var percentage = angle / 360f;
        var shiftedHue = (percentage * 0.75f) + 0.25f; 
        
        return Color.HSVToRGB(shiftedHue, 0.6f, 1f);
    }
}
