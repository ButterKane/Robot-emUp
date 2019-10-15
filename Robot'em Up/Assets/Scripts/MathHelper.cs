using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
	public static float GetAngleBetween2Vectors(Vector2 initial, Vector2 target) //Returns the angle between the two vectors
	{
		Vector2 dir = initial - target;
		float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
		return angle;
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
}
