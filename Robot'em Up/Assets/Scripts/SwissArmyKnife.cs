﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SwissArmyKnife
{
    public static Vector3 GetFlattedDownPosition(Vector3 vector, Vector3 self)
    {
        return new Vector3(vector.x, self.y, vector.z);
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
}
