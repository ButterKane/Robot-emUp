using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SwissArmyKnife
{
    public static Vector3 GetFlattedDownPosition(Vector3 _vector, Vector3 _self)
    {
        return new Vector3(_vector.x, _self.y, _vector.z);
    }
    public static Vector3 GetFlattedDownDirection(Vector3 _vector)
    {
        return new Vector3(_vector.x, 0, _vector.z);
    }

	public static Vector3 RotatePointAroundPivot ( Vector3 _point, Vector3 _pivot, Vector3 _angles )
	{
		Vector3 dir = _point - _pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(_angles) * dir; // rotate it
		_point = dir + _pivot; // calculate rotated point
		return _point; // return it
	}


	public static float GetAngleBetween2Vectors(Vector2 _initial, Vector2 _target) //Returns the angle between the two vectors
    {
        Vector2 dir = _initial - _target;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        return angle;
    }

	public static float SignedAngleBetween ( Vector3 _a, Vector3 _b, Vector3 _n )
	{
		// angle in [0,180]
		float angle = Vector3.Angle(_a, _b);
		float sign = Mathf.Sign(Vector3.Dot(_n, Vector3.Cross(_a, _b)));

		// angle in [-179,180]
		float signed_angle = angle * sign;

		// angle in [0,360] (not used but included here for completeness)
		float angle360 =  (signed_angle + 180) % 360;

		return angle360;
	}

	public static Vector3 GetMouseDirection(Camera _camera, Vector3 _objectPosition) //Returns the direction from the object to the mouse
    {
        Vector3 internal_mousePosInWorld = Input.mousePosition;
        internal_mousePosInWorld.z = Vector3.Distance(_objectPosition, _camera.transform.position);

        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            internal_mousePosInWorld.z = Vector3.Distance(_camera.transform.position, hit.point);
        }
        internal_mousePosInWorld = _camera.ScreenToWorldPoint(internal_mousePosInWorld);
        internal_mousePosInWorld.y = _objectPosition.y;
        return (internal_mousePosInWorld - _objectPosition).normalized;
    }

    public static Vector3 GetBallisticVelocityVector(Vector3 _start, Vector3 _target, float _angle, Vector3 _planReference)
    {
        Vector3 direction = _target - _start;

        float h = direction.y;  // store "height" of start->target direction
        direction.y = 0;

        float distance = direction.magnitude;   // How far is the target from the start?
        float a = _angle * Mathf.Deg2Rad;        // What is the start angle of the parabola
        direction.y = distance * Mathf.Tan(a);  // => distance * (height/distanceToHeight)
        distance += h / Mathf.Tan(a);           // => distance += stored initial height(here will be 0) / (height/distanceToHeight) => distance += 0 (suppposedly in most cases)

        // Calculate velocity
        float velocity = Mathf.Sqrt(distance * Physics.gravity.magnitude / Mathf.Sin(2 * a));
        return velocity * direction.normalized;
    }

	public static Vector3 CubicBezierCurve ( Vector3 _p0, Vector3 _p1, Vector3 _p2, Vector3 _p3, float _t ) //P1 = startPoint, P2 = handle, P3 = second handle, P4 = endPoint, t = time (between 0f and 1f)
	{
		float r = 1f - _t;
		float f0 = r * r * r;
		float f1 = r * r * _t * 3;
		float f2 = r * _t * _t * 3;
		float f3 = _t * _t * _t;
		return f0 * _p0 + f1 * _p1 + f2 * _p2 + f3 * _p3;
	}

    /// <summary>
    /// Returns the distance traveled by an object following ballistic equation
    /// </summary>
    /// <param name="_initialSpeed"></param>
    /// <param name="_angle"></param>
    /// <param name="_initialHeight"></param>
    /// <returns></returns>
    public static float GetBallisticThrowLength(float _initialSpeed, float _angle, float _initialHeight)
    {
        float g = 9.81f; // gravity

        float radAngle = _angle * Mathf.Deg2Rad; // converts the angle in degree to an angle in radians

        if (_initialHeight == 0)
        {
            return (Mathf.Pow(_initialSpeed, 2) / g) * Mathf.Sin(2 * radAngle);  // if initialHieght = 0, use the simplified equation
        }

        //  initial speed X cos(angle)
        //  __________________________
        //              g

        float equationPart1 = (_initialSpeed * Mathf.Cos(radAngle)) / g;


        //                                 _______________________________________________________
        //  initial speed X sin(angle) + \/ (initial speed X sin(angle))² + 2 X g X initial height    

        float equationPart2 = _initialSpeed * Mathf.Sin(radAngle) + Mathf.Sqrt(Mathf.Pow((_initialSpeed * Mathf.Sin(radAngle)), 2) + (2 * g * _initialHeight));

        return equationPart1 * equationPart2;
    }

    /// <summary>
    /// Returns the time taken by an object to travel a given distance with ballistic equation
    /// </summary>
    /// <param name="_initialSpeed"></param>
    /// <param name="_angle"></param>
    /// <param name="_throwLength"></param>
    /// <returns></returns>
    public static float GetBallisticThrowDuration(float _initialSpeed, float _angle, float _throwLength)
    {
        float radAngle = _angle * Mathf.Deg2Rad; // converts the angle in degree to an angle in radians

        return _throwLength / (_initialSpeed * Mathf.Cos(radAngle));
    }

    
}
