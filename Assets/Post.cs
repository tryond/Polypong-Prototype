using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Post : MonoBehaviour, IReflector
{
    [SerializeField] private float noise = 1f;
    public Vector2 GetReflection(Vector2 contactPoint, Vector2 direction)
    {
        // var normal = (contactPoint - (Vector2) transform.position).normalized;
        // // var normal = -transform.up;
        // var reflection = Vector2.Reflect(direction.normalized, normal);
        //
        // Debug.DrawRay(transform.position, 10f * normal, Color.magenta, 3f);

        var reflection = -transform.up;        
        
        return Quaternion.AngleAxis(Random.Range(-noise, noise), Vector3.forward) * reflection;
    }
}
