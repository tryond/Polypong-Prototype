using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

[ExecuteAlways] public class ArenaNode : ImmediateModeShapeDrawer
{
    public bool active = true;

    private Color GetColor()
    {
        return active ? Color.white : Color.red;
    }


    public override void DrawShapes( Camera cam ){

        using( Draw.Command( cam ) ){

            // set up static parameters. these are used for all following Draw.Line calls
            Draw.LineGeometry = LineGeometry.Flat2D;
            Draw.LineThicknessSpace = ThicknessSpace.Pixels;
            Draw.LineThickness = 0.5f; // 4px wide

            // set static parameter to draw in the local space of this object
            Draw.Matrix = transform.localToWorldMatrix;

            var radius = 0.05f;
            var color = GetColor();
            
            Draw.Disc(Vector3.zero, Quaternion.identity, radius, color);
        }

    }
}
