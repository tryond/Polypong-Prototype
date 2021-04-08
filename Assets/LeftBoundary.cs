using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftBoundary : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log(other.gameObject.tag);
    }
}
