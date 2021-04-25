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

    private List<Side> sides;
    
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
        sides = new List<Side>();
        for (int i = 0; i < verticesList.Count; i++)
        {
            var leftVertex = verticesList[i];
            var rightVertex = verticesList[(i + 1) % verticesList.Count];

            var leftPos = leftVertex.transform.position;
            var rightPos = rightVertex.transform.position;

            var position = leftPos + (rightPos - leftPos) / 2f;
            var rotation = Quaternion.LookRotation(Vector3.forward, transform.position - position);
            var length = Vector3.Distance(leftPos, rightPos);
            
            var side = Instantiate(sidePrefab, position, rotation);
            side.SetLength(length);
            side.SetColor(Color.HSVToRGB(startHue + (hueInc * i), 0.6f, 1f));
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
