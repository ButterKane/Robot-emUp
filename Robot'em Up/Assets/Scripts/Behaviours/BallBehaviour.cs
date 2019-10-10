using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : MonoBehaviour
{
    public Transform self;
    public AnimationCurve curveBallSpeed;
    public float curveBallAvancement;
    public float straightBallAvancement;

    // Start is called before the first frame update
    void Start()
    {
        self = transform;
    }

    private void OnEnable()
    {
        self = transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ThrowStraightBall(Vector3 destination, float distanceToTarget, Transform thrower)
    {
        Debug.Log("Throwing straight ball");
        straightBallAvancement = 0;
        StartCoroutine(ApplyStraightBallMovement(destination, distanceToTarget, thrower));
    }

    public IEnumerator ApplyStraightBallMovement(Vector3 destination, float distanceToTarget, Transform thrower)
    {
        Debug.Log("straight ball coroutine");

        Vector3 startPoint = thrower.position;
        Vector3 endPoint = destination;

        Debug.DrawLine(startPoint, endPoint, Color.red, 5f);

        while (straightBallAvancement < 1)
        {
            self.position = Vector3.Lerp(startPoint, destination, straightBallAvancement);
            straightBallAvancement += Time.deltaTime;
            yield return null;
        }
    }

    public void ThrowCurveBall(Vector3 destination, float distanceToTarget, Transform thrower)
    {
        Debug.Log("Throwing curve ball");
        float throwAngle = Vector3.SignedAngle(thrower.forward, destination - thrower.position, Vector3.up);
        curveBallAvancement = 0;
        Debug.Log("throwAngle: " + throwAngle);

        float throwSens = throwAngle > 0 ? -1 : 1;
        //StartCoroutine(ApplyCurveBallMovement(destination, distanceToTarget, thrower, throwAngle));
        StartCoroutine(FollowArc(self, thrower.position, destination, throwSens, 5f));
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
        
        do
        {
            float angle = startAngle + curveBallAvancement * travel;
            movingObject.position = center3D + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * absRadius;
            Debug.DrawRay(self.position, Vector3.up, Color.red, 7);
            curveBallAvancement += Time.deltaTime / duration;
            yield return null;
        } while (curveBallAvancement < 1f);

        movingObject.position = end;
    }
}
