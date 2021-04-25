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
        sideList = new List<Side>();
        base.Awake();
    }

    protected void Start()
    {
        for (int i = 0; i < vertexList.Count; i++)
        {
            var side = Instantiate(sidePrefab);
            sideList.Add(side);
        }
        UpdateSidePositions();
       
        // setup paddles
        // var player = Instantiate(playerPaddlePrefab);
        // sides[0].SetPaddle(player);
        // for (int i = 1; i < sides.Count; i++)
        // sides[i].SetPaddle(Instantiate(enemyPaddlePrefab));
        // setup sides
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
    
    protected override bool Split(int vertexIndex)
    {
        var vertexAdded = base.Split(vertexIndex);
        if (!vertexAdded)
            return false;
        
        // instantiate off-camera
        var side = Instantiate(sidePrefab, new Vector3(1000f, 1000f, 0f), Quaternion.identity);
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

    private Color GetColorFromNormal(Vector3 normal)
    {
        var angle = Vector3.SignedAngle(Vector3.right, normal, Vector3.forward);
        var percentage = angle / 360f;
        var shiftedHue = (percentage * 0.75f) + 0.25f; 
        
        return Color.HSVToRGB(shiftedHue, 0.6f, 1f);
    }
}
