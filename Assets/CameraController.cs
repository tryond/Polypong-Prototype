using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Camera arenaCam;
    
    [SerializeField] [CanBeNull] private Side playerSide;
    [SerializeField] private Arena arena;
    
    [SerializeField] [CanBeNull] private GameObject offscreenIndicatorPrefab;
    
    [SerializeField] private float topBuffer = 2f;
    [SerializeField] private float sideBuffer = 0.5f;
    [SerializeField] private float bottomBuffer = 3f;
    [SerializeField] private float thumbBuffer = 0.5f;

    private Vector2 topViewportPos = Vector2.zero;
    private Vector2 bottomViewportPos = Vector2.zero;
    
    private Vector3 inPosition;
    private float inOrtho;

    private Vector3 outPosition;
    private float outOrtho;
    
    private Vector3 targetPosition;
    private float targetOrtho;
    
    [SerializeField] private float smoothTime = 0.5f;
    [SerializeField] private float speed = 100;
    
    private Vector3 positionVelocity = Vector3.zero;
    private float orthoVelocity = 0f;

    private Dictionary<Ball, GameObject> ballToOffscreenIndicator;

    private bool zoomedOut = true;

    private float p;
    
    void Start()
    {
        // cam = Camera.main; this should just be passed in

        
        
        targetOrtho = outOrtho;    // this is wrong -- arena radius is not set at this point
        targetPosition = outPosition;
        
        ballToOffscreenIndicator = new Dictionary<Ball, GameObject>();
    }

    public void BallIncoming(Ball ball)
    {
        // Debug.Log("Incoming!");
        
        if (ballToOffscreenIndicator.ContainsKey(ball))
            return;
        
        ballToOffscreenIndicator[ball] =
            Instantiate(offscreenIndicatorPrefab, new Vector3(1000f, 1000f, 0f), Quaternion.identity);
    }

    public void BallOutgoing(Ball ball)
    {
        // Debug.Log("Outgoing!");
        
        if (!ballToOffscreenIndicator.ContainsKey(ball))
            return;

        var indicator = ballToOffscreenIndicator[ball];
        ballToOffscreenIndicator.Remove(ball);
        Destroy(indicator);
    }

    private void PlaceOffscreenIndicators()
    {
        if (!playerSide)
            return;
        
        // foreach (var ball in (BallManager.instance.balls))
        foreach (var ballAndIndicator in ballToOffscreenIndicator)
        {
            var ball = ballAndIndicator.Key;
            var indicator = ballAndIndicator.Value;
            
            Vector2 intersection = Vector2.zero;
            if (IsOnscreen(ball.gameObject) || !GetBallFrustumIntercept(out intersection, ball))
            {
                indicator.SetActive(false);
                continue;
            }
            
            indicator.SetActive(true);
            indicator.transform.position = intersection;
            indicator.transform.rotation = Quaternion.LookRotation(Vector3.forward, ball.direction);
        }
        
    }

    private bool GetBallFrustumIntercept(out Vector2 intersection, Ball ball)
    {
        Vector2 ballStartWorld = ball.transform.position;
        Vector2 ballEndWorld = (1000f * ball.direction.normalized) + ballStartWorld;
        
        Debug.DrawLine(ballStartWorld, ballEndWorld, Color.red, Time.fixedDeltaTime);
        
        var bottomLeft = new Vector2(sideBuffer, 0f);
        var topLeft = new Vector2(sideBuffer, 1f - sideBuffer);
        var topRight = new Vector2(1f - sideBuffer, 1f - sideBuffer);
        var bottomRight = new Vector2(1f - sideBuffer, 0f);

        var ballStart = cam.WorldToViewportPoint(ballStartWorld);
        var ballEnd = cam.WorldToViewportPoint(ballEndWorld);
        
        var isAbove = ballStart.y > 1f;
        var isLeft = ballStart.x < 0f;
        var isRight = ballStart.x > 1f;

        // top
        if (isAbove && LineIntersection(out intersection, ballStart, ballEnd, topLeft, topRight))
        {
            intersection = cam.ViewportToWorldPoint(intersection);
            return true;
        }
        
        // left
        if (isLeft && LineIntersection(out intersection, ballStart, ballEnd, bottomLeft, topLeft))
        {
            intersection = cam.ViewportToWorldPoint(intersection);
            return true;
        }
        
        // right
        if (isRight && LineIntersection(out intersection, ballStart, ballEnd, topRight, bottomRight))
        {
            intersection = cam.ViewportToWorldPoint(intersection);
            return true;
        }

        Debug.Log("No Intercept!");
        
        intersection = Vector2.zero;
        return false;
    }

    //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
    //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
    //same plane, use ClosestPointsOnTwoLines() instead.
    public static bool LineLineIntersection(out Vector2 intersection, Vector2 linePoint1, Vector2 lineVec1, Vector2 linePoint2, Vector2 lineVec2){
 
        Vector2 lineVec3 = linePoint2 - linePoint1;
        Vector2 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector2 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);
 
        float planarFactor = Vector2.Dot(lineVec3, crossVec1and2);
 
        //is coplanar, and not parrallel
        if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector2.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector2.zero;
            return false;
        }
    }
    
    public static bool LineIntersection(out Vector2 intersection, Vector2 p1,Vector2 p2, Vector2 p3, Vector2 p4)
    {
        intersection = Vector2.zero;
        
        float Ax,Bx,Cx,Ay,By,Cy,d,e,f,num/*,offset*/;
        float x1lo,x1hi,y1lo,y1hi;
 
        Ax = p2.x-p1.x;
        Bx = p3.x-p4.x;
 
        // X bound box test/
        if(Ax<0) {
            x1lo=p2.x; x1hi=p1.x;
        } else {
            x1hi=p2.x; x1lo=p1.x;
        }
 
        if(Bx>0) {
            if(x1hi < p4.x || p3.x < x1lo) return false;
        } else {
            if(x1hi < p3.x || p4.x < x1lo) return false;
        }
 
        Ay = p2.y-p1.y;
        By = p3.y-p4.y;
 
        // Y bound box test//
        if(Ay<0) {                  
            y1lo=p2.y; y1hi=p1.y;
        } else {
            y1hi=p2.y; y1lo=p1.y;
        }
 
        if(By>0) {
            if(y1hi < p4.y || p3.y < y1lo) return false;
        } else {
            if(y1hi < p3.y || p4.y < y1lo) return false;
        }
 
        Cx = p1.x-p3.x;
        Cy = p1.y-p3.y;
 
        d = By*Cx - Bx*Cy;  // alpha numerator//
        f = Ay*Bx - Ax*By;  // both denominator//
 
        // alpha tests//
        if(f>0) {
            if(d<0 || d>f) return false;
        } else {
            if(d>0 || d<f) return false;
        }
 
        e = Ax*Cy - Ay*Cx;  // beta numerator//
 
        // beta tests //
        if(f>0) {                          
            if(e<0 || e>f) return false;
        } else {
            if(e>0 || e<f) return false;
        }
 
        // check if they are parallel
        if(f==0) return false;
       
        // compute intersection coordinates //
        num = d*Ax; // numerator //
 
        intersection.x = p1.x + num / f;
        num = d*Ay;
        intersection.y = p1.y + num / f;
        return true;
    }
    
    // // Returns 1 if the lines intersect, otherwise 0. In addition, if the lines 
    // // intersect the intersection point may be stored in the floats i_x and i_y.
    // char get_line_intersection(float p0_x, float p0_y, float p1_x, float p1_y, 
    //     float p2_x, float p2_y, float p3_x, float p3_y, float *i_x, float *i_y)
    // {
    //     float s1_x, s1_y, s2_x, s2_y;
    //     s1_x = p1_x - p0_x;     s1_y = p1_y - p0_y;
    //     s2_x = p3_x - p2_x;     s2_y = p3_y - p2_y;
    //
    //     float s, t;
    //     s = (-s1_y * (p0_x - p2_x) + s1_x * (p0_y - p2_y)) / (-s2_x * s1_y + s1_x * s2_y);
    //     t = ( s2_x * (p0_y - p2_y) - s2_y * (p0_x - p2_x)) / (-s2_x * s1_y + s1_x * s2_y);
    //
    //     if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
    //     {
    //         // Collision detected
    //         if (i_x != NULL)
    //             *i_x = p0_x + (t * s1_x);
    //         if (i_y != NULL)
    //             *i_y = p0_y + (t * s1_y);
    //         return 1;
    //     }
    //
    //     return 0; // No collision
    // }
    
    private bool IsOnscreen(GameObject gameObject)
    {
        Vector3 screenPoint = cam.WorldToViewportPoint(gameObject.transform.position);
        return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }

    private void LateUpdate()
    {
        UpdateArenaView(out outOrtho, out outPosition);
        UpdatePlayerView(out inOrtho, out inPosition);

        // if (zoomedOut || !playerSide)
        // {
        //     targetOrtho = outOrtho;
        //     targetPosition = outPosition;
        // }
        // else
        // {
        //     targetOrtho = inOrtho;
        //     targetPosition = inPosition;
        // }
        
        // if (Vector3.Distance(cam.transform.position, targetPosition) < 0.01f)
        //     return;
        
        targetOrtho = Mathf.Lerp(inOrtho, outOrtho, p);
        targetPosition = Vector3.Lerp(inPosition, outPosition, p);
        
        // Move camera towards position
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPosition, ref positionVelocity, smoothTime, speed);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetOrtho, ref orthoVelocity, smoothTime, speed);
        
        cam.transform.up = playerSide ? playerSide.transform.up : arena.transform.up;
        arenaCam.transform.up = cam.transform.up;
        
        PlaceOffscreenIndicators();
        DrawBounds();
    }
    
    private void UpdatePlayerView(out float ortho, out Vector3 position)
    {
        if (!playerSide)
        {
            ortho = 0f;
            position = Vector3.zero;
            return;
        }
        
        ortho = playerSide.Length / (2f * cam.aspect);

        var direction = playerSide.transform.up;
        position = playerSide.transform.position + (ortho - bottomBuffer) * direction;
        position = new Vector3(position.x, position.y, -10f);

        bottomViewportPos = new Vector2(0f, bottomBuffer / (2f * ortho));
    }

    private void UpdateArenaView(out float ortho, out Vector3 position)
    {
        ortho = (arena.Radius / cam.aspect) + (sideBuffer * 2f);

        // var direction = playerSide ? playerSide.transform.up : arena.transform.up;

        // position = arena.transform.position - (ortho - arena.Radius - topBuffer) * direction;
        position = arena.transform.position;
        position = new Vector3(position.x, position.y, -10f);

        topViewportPos = new Vector2(0f, 1f - ((2f * arena.Radius + topBuffer) / (2f * ortho)));
    }

    public void ToggleZoom()
    {
        zoomedOut = (bool) playerSide && !zoomedOut;
    }

    public void DrawBounds()
    {
        var bottomLeft = cam.ViewportToWorldPoint(new Vector2(sideBuffer, 0f));
        var topLeft = cam.ViewportToWorldPoint(new Vector2(sideBuffer, 1f - sideBuffer));
        var topRight = cam.ViewportToWorldPoint(new Vector2(1f - sideBuffer, 1f - sideBuffer));
        var bottomRight = cam.ViewportToWorldPoint(new Vector2(1f - sideBuffer, 0f));

        Debug.DrawLine(bottomLeft, topLeft, Color.magenta, Time.deltaTime);
        Debug.DrawLine(topLeft, topRight, Color.magenta, Time.deltaTime);
        Debug.DrawLine(topRight, bottomRight, Color.magenta, Time.deltaTime);
    }

    public void SetCameraZoom(Vector3 position)
    {
        var viewportPoint = cam.WorldToViewportPoint(position);

        Debug.DrawLine(cam.ViewportToWorldPoint(topViewportPos), cam.ViewportToWorldPoint(bottomViewportPos), Color.green, Time.deltaTime);
        
        var p = Mathf.Clamp((viewportPoint.y - bottomViewportPos.y) / (topViewportPos.y - bottomViewportPos.y), 0f, 1f);

        targetOrtho = Mathf.Lerp(inOrtho, outOrtho, p);
        targetPosition = Vector3.Lerp(inPosition, outPosition, p);
        
        Debug.Log($"Zoom: {p}");
    }

    public void Zoom(Vector3 fromWorldPoint, Vector3 toWorldPoint)
    {
        var fromViewportPoint = cam.WorldToViewportPoint(fromWorldPoint);
        var toViewportPoint = cam.WorldToViewportPoint(toWorldPoint);
        
        // 0.50 viewport -> 0.25 zoom
        p -= (fromViewportPoint.y - toViewportPoint.y);
    }
    
}