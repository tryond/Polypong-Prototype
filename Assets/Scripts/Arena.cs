using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Arena : DynamicPolygon
{
    [SerializeField] private Side sidePrefab;
    [SerializeField] private Paddle playerPaddlePrefab;
    [SerializeField] private Paddle enemyPaddlePrefab;

    private List<Side> sides;
    
    public UnityEvent<int> OnNumSidesChanged = new UnityEvent<int>();

    protected override void Awake()
    {
        // setup vertices
        base.Awake();
        
        // setup sides
        sides = new List<Side>();
        for (int i = 0; i < verticesList.Count; i++)
        {
            var side = Instantiate(sidePrefab);
            // side.SetBounds(verticesList[i], verticesList[(i + 1) % verticesList.Count]);
            sides.Add(side);
        }
        
        // setup paddles
        // var player = Instantiate(playerPaddlePrefab);
        // sides[0].SetPaddle(player);
        // for (int i = 1; i < sides.Count; i++)
            // sides[i].SetPaddle(Instantiate(enemyPaddlePrefab));
    }

    protected override void RemoveCollapsedVertices()
    {
        // destroy collapsed sides
        foreach (var vertexHash in collapseVertices.ToList())
        {
            var vertex = verticesMap[vertexHash];
            var sideIndex = (verticesList.IndexOf(vertex) - 1) % verticesList.Count;

            var side = sides[sideIndex];
            sides.RemoveAt(sideIndex);
            
            Destroy(side.gameObject);
        }
        base.RemoveCollapsedVertices();
    }
}
