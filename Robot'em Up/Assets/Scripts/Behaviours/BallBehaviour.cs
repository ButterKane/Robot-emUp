﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : MonoBehaviour
{
    public Transform self;
    public AnimationCurve curveBallSpeed;
    public float curveBallAvancement;
    public float straightBallAvancement;
    public Rigidbody rb;
    public GameObject thrower;
    public float magnetForceRatio = 10f;

    // Start is called before the first frame update
    void Start()
    {
        self = transform;
    }

    private void OnEnable()
    {
        self = transform;
        rb = GetComponent<Rigidbody>();
        GameManager.i.ball = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ThrowStraightBall(Vector3 destination, float distanceToTarget, Transform throwingPlayer)
    {
        thrower = throwingPlayer.gameObject;

        straightBallAvancement = 0;
        StartCoroutine(ApplyStraightBallMovement(destination, distanceToTarget, throwingPlayer));
    }

    public IEnumerator ApplyStraightBallMovement(Vector3 destination, float distanceToTarget, Transform thrower)
    {
        Vector3 startPoint = self.position;
        Vector3 endPoint = destination;

        Debug.DrawLine(startPoint, endPoint, Color.red, 5f);

        while (straightBallAvancement < 1)
        {
            self.position = Vector3.Lerp(startPoint, destination, straightBallAvancement);
            straightBallAvancement += Time.deltaTime;
            yield return null;
        }

        EndOfPass((endPoint - startPoint));
    }

    public void ThrowCurveBall(Vector3 destination, float distanceToTarget, Transform throwingPlayer)
    {
        thrower = throwingPlayer.gameObject;

        float throwAngle = Vector3.SignedAngle(throwingPlayer.forward, destination - throwingPlayer.position, Vector3.up);
        curveBallAvancement = 0;
        float throwSens = throwAngle > 0 ? 1 : -1;
        
        throwAngle = Mathf.Clamp(Mathf.Abs(throwAngle), 0, 90);

        float radius = (90 / throwAngle) * (distanceToTarget / 2);
        
        //StartCoroutine(ApplyCurveBallMovement(destination, distanceToTarget, thrower, throwAngle));
        StartCoroutine(FollowArc(self, self.position, destination, throwSens* radius, 5f));
    }

    public IEnumerator ApplyCurveBallMovement(Vector3 destination, float distanceToTarget, Transform thrower, float throwAngle)
    {
        Debug.Log("curve ball coroutine");
        
        float throwHeight = Mathf.Tan(throwAngle) * (distanceToTarget / 2);
        float throwSide = throwAngle > 0 ? -1 : 1;
        
        Vector3 highPoint = thrower.position + (destination - thrower.position) / 2 + throwSide *Vector3.right * throwHeight; // Play with 5.0 to change the curve
        
        while (curveBallAvancement < 1)
        {
            // Get 
            Vector3 m1 = Vector3.Lerp(thrower.position, highPoint, curveBallAvancement);
            Vector3 m2 = Vector3.Lerp(highPoint, destination, curveBallAvancement);
            self.position = Vector3.Lerp(m1, m2, curveBallAvancement);

            Debug.DrawRay(self.position, Vector3.up, Color.red, 7);

            curveBallAvancement += Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator FollowArc(Transform movingObject,Vector3 startPoint,Vector3 endPoint,float throwSens,float duration)
    {
        Vector2 start = new Vector2(startPoint.x, startPoint.z);
        Vector2 end = new Vector2(endPoint.x, endPoint.z);
        
        Vector3 lastPos1 = startPoint;
        Vector3 lastPos2 = endPoint;

        Vector2 difference = end - start;
        float span = difference.magnitude;

        // Override the radius if it's too small to bridge the points.
        float absRadius = Mathf.Abs(throwSens);
        if (span > 2f * absRadius)
            throwSens = absRadius = span / 2f;

        Vector2 perpendicular = new Vector2(difference.y, -difference.x) / span;
        perpendicular *= Mathf.Sign(throwSens) * Mathf.Sqrt(throwSens * throwSens - span * span / 4f);
        

        Vector2 center = start + difference / 2f + perpendicular;

        Vector2 toStart = start - center;
        float startAngle = Mathf.Atan2(toStart.y, toStart.x);

        Vector2 toEnd = end - center;
        float endAngle = Mathf.Atan2(toEnd.y, toEnd.x);

        // Choose the smaller of two angles separating the start & end
        float travel = (endAngle - startAngle + 5f * Mathf.PI) % (2f * Mathf.PI) - Mathf.PI;

        Vector3 center3D = new Vector3(center.x, movingObject.position.y, center.y);

        int ballSpeed = 3; // Very ugly, needs to be changed
        do
        {
            float actualSpeed = 0;
            do
            {
                lastPos1 = movingObject.position;
                float angle = startAngle + curveBallAvancement * travel;
                movingObject.position = center3D + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * absRadius;

                
                Debug.DrawRay(self.position, Vector3.up, Color.red, 7);
                actualSpeed++;
                curveBallAvancement += Time.deltaTime / duration;
            } while (actualSpeed < ballSpeed);

            lastPos2 = movingObject.position;
            yield return null;
        } while (curveBallAvancement < 1f);

        movingObject.position = endPoint;
        
        EndOfPass((lastPos2 - lastPos1) * 150); // 150 is to make the propulsion "present". or else it's not strong enough
    }

    public void EndOfPass(Vector3 movementDirection)
    {
        Debug.Log("End of pass");
        rb.useGravity = true;
        rb.AddForce(movementDirection * 25, ForceMode.Acceleration);
        thrower = null;
    }

    public void OnCollisionEnter(Collision collision)
    {
        StopAllCoroutines();
        rb.useGravity = true;
        rb.AddForce(Vector3.up, ForceMode.Impulse);
    }

    public void ActivateMagnet(Transform magnetPlayer)
    {
        Debug.Log("Activate magnet for " + magnetPlayer.name);
        if (magnetPlayer.gameObject != thrower)
        {
            StartCoroutine(MagnetAttraction(magnetPlayer));
        }
        else
        {
            Debug.Log(magnetPlayer.gameObject.name + " is the thrower, don't magnet to it");
        }
    }

    public IEnumerator MagnetAttraction(Transform magnetPlayer)
    {
        float distance;
        do
        {
            rb.velocity = rb.velocity*0.99f;
            Vector3 fromBallToPlayer = magnetPlayer.position - self.position;
            distance = (fromBallToPlayer).magnitude;

            rb.AddForce(fromBallToPlayer.normalized * (magnetForceRatio / distance), ForceMode.Acceleration);

            Debug.DrawLine(self.position, magnetPlayer.position, Color.blue);

            yield return null;
        } while (distance > 0.1f);
        
    }
}
