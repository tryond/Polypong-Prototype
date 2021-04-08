using System;
using UnityEngine;

[Serializable] public class Polygon
{
    public (Vector3 left, Vector3 right)[] Positions { get; private set; }
    public float SideLength { get; private set; }
    public float Radius { get; }
    public Vector3[] Points { get; private set; }

    public Polygon(int numSides, float radius)
    {
        this.Radius = radius;
        Setup(numSides, radius);
    }

    private void Setup(int numSides, float radius)
    {
        Positions = new (Vector3, Vector3)[numSides];
        switch (numSides)
        {
            case 2:
                Points = CalculatePoints(4, radius);
                Positions[0] = (Points[0], Points[1]);
                Positions[1] = (Points[2], Points[3]);
                break;
            default:
                Points = CalculatePoints(numSides, radius);
                for (int i = 0; i < numSides; ++i)
                    Positions[i] = (left: Points[i], right: Points[(i + 1) % numSides]);
                break;
        }
    }

    private Vector3[] CalculatePoints(int numSides, float radius)
    {
        if (numSides <= 1)
            return new Vector3[] { Vector3.zero };

        // determine base vertex which to rotate around circle
        float theta = 360f / numSides;
        SideLength = (float)(2f * radius * Math.Tan((theta * Math.PI) / 360f));
        // Vector3 baseVertex = Quaternion.Euler(0f, 0f, -theta / 2) * new Vector3(0f, -radius, 0f);
        Vector3 baseVertex = new Vector3(radius, 0f, 0f);

        // calculate points around circle
        var points = new Vector3[numSides];
        for (var i = 0; i < numSides; ++i)
            points[i] = Quaternion.Euler(0f, 0f, theta * i) * baseVertex;

        return points;
    }
}
