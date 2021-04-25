using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Arena : DynamicPolygon
{
    [SerializeField] private Side sidePrefab;
    [SerializeField] private Paddle playerPaddlePrefab;
    [SerializeField] private Paddle enemyPaddlePrefab;

    private List<Side> sideList;
    
    public UnityEvent<int> OnNumSidesChanged = new UnityEvent<int>();

    protected override void Awake()
    {
        // setup vertices
        base.Awake();
        
        // TODO: should I override move to positions ??
        
        // set color increments
        var hueInc = 1f / numSides;
        var startHue = hueInc / 2f;
        
        // setup sides
        sideList = new List<Side>();
        for (int i = 0; i < vertexList.Count; i++)
        {
            var side = Instantiate(sidePrefab);
            side.SetColor(Color.HSVToRGB(startHue + (hueInc * i), 0.6f, 1f));
            sideList.Add(side);
        }
        UpdateSidePositions();
        
        // setup paddles
        // var player = Instantiate(playerPaddlePrefab);
        // sides[0].SetPaddle(player);
        // for (int i = 1; i < sides.Count; i++)
            // sides[i].SetPaddle(Instantiate(enemyPaddlePrefab));
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
        }
    }
    
    protected override bool Split(int vertexIndex)
    {
        var vertexAdded = base.Split(vertexIndex);
        if (!vertexAdded)
            return false;
        
        var side = Instantiate(sidePrefab);
        side.SetColor(Color.green);    // TODO: not sure what color to set this to...
        sideList.Insert(vertexIndex, side);
        
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
            var vertex = vertexMap[vertexHash];
            var sideIndex = vertexList.IndexOf(vertex);
            
            var side = sideList[sideIndex];
            sideList.RemoveAt(sideIndex);
            
            Destroy(side.gameObject);
        }
        base.RemoveCollapsedVertices();
    }
}
