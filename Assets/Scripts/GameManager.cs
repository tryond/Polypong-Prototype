using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int targetFrameRate = 60;
    public bool cursorVisible = false;
    public Camera cam;
    public GameObject player;
    
    public void Start()
    {
        Application.targetFrameRate = targetFrameRate;
        Cursor.visible = cursorVisible;
    }

    public void LateUpdate()
    {
        cam.transform.rotation = Quaternion.LookRotation(Vector3.forward, player.transform.up);
    }

    public void Restart()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
    
}
