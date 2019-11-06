using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SwissArmyKnife
{
    public static Vector3 GetFlattedDownPosition(Vector3 vector, Vector3 self)
    {
        return new Vector3(vector.x, self.y, vector.z);
    }
    public static Vector3 GetFlattedDownDirection(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }


        public static float GetAngleBetween2Vectors(Vector2 initial, Vector2 target) //Returns the angle between the two vectors
    {
        Vector2 dir = initial - target;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        return angle;
    }

	public static float SignedAngleBetween ( Vector3 a, Vector3 b, Vector3 n )
	{
		// angle in [0,180]
		float angle = Vector3.Angle(a, b);
		float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

		// angle in [-179,180]
		float signed_angle = angle * sign;

		// angle in [0,360] (not used but included here for completeness)
		float angle360 =  (signed_angle + 180) % 360;

		return angle360;
	}

	public static Vector3 GetMouseDirection(Camera camera, Vector3 objectPosition) //Returns the direction from the object to the mouse
    {
        Vector3 mousePosInWorld = Input.mousePosition;
        mousePosInWorld.z = Vector3.Distance(objectPosition, camera.transform.position);
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            mousePosInWorld.z = Vector3.Distance(camera.transform.position, hit.point);
        }
        mousePosInWorld = camera.ScreenToWorldPoint(mousePosInWorld);
        mousePosInWorld.y = objectPosition.y;
        return (mousePosInWorld - objectPosition).normalized;
    }

    public static Vector3 GetBallisticVelocityVector(Vector3 start, Vector3 target, float angle, Vector3 planReference)
    {
        Vector3 direction = target - start;

        float h = direction.y;  // store "height" of start->target direction
        direction.y = 0;

        float distance = direction.magnitude;   // How far is the target from the start?
        float a = angle * Mathf.Deg2Rad;        // What is the start angle of the parabola
        direction.y = distance * Mathf.Tan(a);  // => distance * (height/distanceToHeight)
        distance += h / Mathf.Tan(a);           // => distance += stored initial height(here will be 0) / (height/distanceToHeight) => distance += 0 (suppposedly in most cases)

        // Calculate velocity
        float velocity = Mathf.Sqrt(distance * Physics.gravity.magnitude / Mathf.Sin(2 * a));
        return velocity * direction.normalized;
    }

	public static Vector3 CubicBezierCurve ( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t ) //P1 = startPoint, P2 = handle, P3 = second handle, P4 = endPoint, t = time (between 0f and 1f)
	{
		float r = 1f - t;
		float f0 = r * r * r;
		float f1 = r * r * t * 3;
		float f2 = r * t * t * 3;
		float f3 = t * t * t;
		return f0 * p0 + f1 * p1 + f2 * p2 + f3 * p3;
	}

    /// <summary>
    /// Returns the distance traveled by an object following ballistic equation
    /// </summary>
    /// <param name="initialSpeed"></param>
    /// <param name="angle"></param>
    /// <param name="initialHeight"></param>
    /// <returns></returns>
    public static float GetBallisticThrowLength(float initialSpeed, float angle, float initialHeight)
    {
        float g = 9.81f; // gravity

        float radAngle = angle * Mathf.Deg2Rad; // converts the angle in degree to an angle in radians

        if (initialHeight == 0)
        {
            return (Mathf.Pow(initialSpeed, 2) / g) * Mathf.Sin(2 * radAngle);  // if initialHieght = 0, use the simplified equation
        }

        //  initial speed X cos(angle)
        //  __________________________
        //              g

        float equationPart1 = (initialSpeed * Mathf.Cos(radAngle)) / g;


        //                                 _______________________________________________________
        //  initial speed X sin(angle) + \/ (initial speed X sin(angle))² + 2 X g X initial height    

        float equationPart2 = initialSpeed * Mathf.Sin(radAngle) + Mathf.Sqrt(Mathf.Pow((initialSpeed * Mathf.Sin(radAngle)), 2) + (2 * g * initialHeight));

        return equationPart1 * equationPart2;
    }

    /// <summary>
    /// Returns the time taken by an object to travel a given distance with ballistic equation
    /// </summary>
    /// <param name="initialSpeed"></param>
    /// <param name="angle"></param>
    /// <param name="throwLength"></param>
    /// <returns></returns>
    public static float GetBallisticThrowDuration(float initialSpeed, float angle, float throwLength)
    {
        float radAngle = angle * Mathf.Deg2Rad; // converts the angle in degree to an angle in radians

        return throwLength / (initialSpeed * Mathf.Cos(radAngle));
    }

    
}
