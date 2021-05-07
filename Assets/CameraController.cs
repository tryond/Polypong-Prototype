using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MoreMountains.NiceVibrations;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] [CanBeNull] private Side playerSide;
    [SerializeField] private Arena arena;
    [SerializeField] [CanBeNull] private GameObject offscreenIndicatorPrefab;
    [SerializeField] private float sideBuffer = 0f;
    
    private Vector3 inPosition;
    private float inOrtho;

    private Vector3 outPosition;
    private float outOrtho;
    
    private Vector3 targetPosition;
    private float targetOrtho;
    
    [SerializeField] private float smoothTime = 0.5f;
    [SerializeField] private float speed = 15;
    
    private Vector3 positionVelocity = Vector3.zero;
    private float orthoVelocity = 0f;

    private Dictionary<Ball, GameObject> ballToOffscreenIndicator;
    
    void Start()
    {
        cam = Camera.main;

        SetInOutProps();
        
        targetOrtho = outOrtho;
        targetPosition = outPosition;
        
        ballToOffscreenIndicator = new Dictionary<Ball, GameObject>();
    }

    public void BallIncoming(Ball ball)
    {
        ball.gameObject.GetComponent<Waypoint_Indicator>().enabled = true;
    }

    public void BallOutgoing(Ball ball)
    {
        ball.gameObject.GetComponent<Waypoint_Indicator>().enabled = false;
    }

    private void LateUpdate()
    {
        if (!playerSide)
            ZoomOut();
        else
            cam.transform.up = playerSide.transform.up;
        
        if (Vector3.Distance(cam.transform.position, targetPosition) < 0.01f)
            return;
        
        // Move camera towards position
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPosition, ref positionVelocity, smoothTime, speed);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetOrtho, ref orthoVelocity, smoothTime, speed);
    }
    
    private void SetInOutProps()
    {
        var aspect = cam.aspect;
        
        inOrtho = playerSide.Length / (2f * aspect);
        inPosition = (inOrtho * 0.5f * playerSide.transform.up) + playerSide.transform.position;
        inPosition = new Vector3(inPosition.x, inPosition.y, -10f);

        outOrtho = (arena.Radius / aspect) + (sideBuffer * 2f);
        outPosition = arena.transform.position;
        outPosition = new Vector3(outPosition.x, outPosition.y, -10f);
    }

    public void ZoomIn()
    {
        if (!playerSide)
            return;
        
        SetInOutProps();
        targetOrtho = inOrtho;
        targetPosition = inPosition;
        cam.transform.SetParent(playerSide.transform);
    }

    public void ZoomOut()
    {
        SetInOutProps();
        targetOrtho = outOrtho;
        targetPosition = outPosition;
        cam.transform.SetParent(arena.transform);
    }
    
    
}