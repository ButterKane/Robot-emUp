using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SnapType
{
	Pass
}
public class SnapController : MonoBehaviour
{
	public static List<Snap> snappedObjects = new List<Snap>();

	//Type = What do you want to snap ?
	//Object = Which object should be snapped 
	//Angle Treshold = How sensitive is the snap (In degrees)
	//Duration = For how long the object must be snappable ? Negative value means infinite (In sec)
	public static void SetSnappable(SnapType _type, GameObject _object, float _angleTreshold, float _duration = -1)
	{
		//Add snappable script to a gameObject
		Snap newSnap = _object.AddComponent<Snap>();
		newSnap.snapType = _type;
		newSnap.duration = _duration;
		if (newSnap.duration < 0)
		{
			newSnap.isInfinite = true;
		}
		newSnap.angleTreshold = _angleTreshold;
	}

	public static Vector3 SnapDirection ( Vector3 _startPos, Vector3 _direction )
	{
		bool snapped;
		return SnapDirection(_startPos, _direction, out snapped);
	}
	public static Vector3 SnapDirection(Vector3 _startPos, Vector3 _direction, out bool _snapped)
	{
		Vector2 v2dir = new Vector2(_direction.x, _direction.z);
		float currentAngle = Vector3.Angle(_direction, new Vector3(0,0,1));
		float currentSign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(_direction, new Vector3(0, 0, 1))));
		Snap snapFound = null;
		foreach (Snap snap in snappedObjects)
		{
			float snapSign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(snap.transform.position - _startPos, new Vector3(0, 0, 1))));
			float snapAngle = Vector3.Angle(snap.transform.position - _startPos, new Vector3(0,0,1));
			float angleDelta = Mathf.Abs(snapAngle - currentAngle);
			if (snapSign == currentSign && angleDelta <= snap.angleTreshold)
			{
				snapFound = snap;
			}
			//Debug.Log("Current angle: " + currentAngle + " Sign: " + currentSign + " | Snap angle: " + snapAngle + " Sign: " + snapSign);
		}
		if (snapFound != null)
		{
			_snapped = true;
			return snapFound.transform.position - _startPos;
		}
		_snapped = false;
		return _direction;
	}
}
