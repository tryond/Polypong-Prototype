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
    public Arena arena;
    public float smoothTime = 0.5f;
    public float sideBuffer = 0.5f;
    
    private float _targetOrtho;
    private float _velocity;

    
    public void Start()
    {
        Application.targetFrameRate = targetFrameRate;
        Cursor.visible = cursorVisible;
        // _targetOrtho = cam.orthographicSize;
    }

    // public void LateUpdate()
    // {
    //     if (player)
    //         cam.transform.rotation = Quaternion.LookRotation(Vector3.forward, player.transform.up);
    //
    //     // _targetOrtho = arena.GetDiameter() / (2f * cam.aspect);
    //     _targetOrtho = (arena.Radius / cam.aspect) + (sideBuffer * 2f);
    //     cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, _targetOrtho, ref _velocity, smoothTime);
    // }

    public void Restart()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
    
}
