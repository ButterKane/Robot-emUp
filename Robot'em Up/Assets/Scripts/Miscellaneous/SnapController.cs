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
		Snap internal_newSnap = _object.AddComponent<Snap>();
		internal_newSnap.snapType = _type;
		internal_newSnap.duration = _duration;
		if (internal_newSnap.duration < 0)
		{
			internal_newSnap.isInfinite = true;
		}
		internal_newSnap.angleTreshold = _angleTreshold;
	}

	public static Vector3 SnapDirection ( Vector3 _startPos, Vector3 _direction )
	{
		bool internal_snapped;
		return SnapDirection(_startPos, _direction, out internal_snapped);
	}
	public static Vector3 SnapDirection(Vector3 _startPos, Vector3 _direction, out bool _snapped)
	{
		Vector2 internal_v2dir = new Vector2(_direction.x, _direction.z);
		float internal_currentAngle = Vector3.Angle(_direction, new Vector3(0,0,1));
		float internal_currentSign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(_direction, new Vector3(0, 0, 1))));
		Snap internal_snapFound = null;
		foreach (Snap snap in snappedObjects)
		{
			float internal_snapSign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(snap.transform.position - _startPos, new Vector3(0, 0, 1))));
			float internal_snapAngle = Vector3.Angle(snap.transform.position - _startPos, new Vector3(0,0,1));
			float internal_angleDelta = Mathf.Abs(internal_snapAngle - internal_currentAngle);
			if (internal_snapSign == internal_currentSign && internal_angleDelta <= snap.angleTreshold)
			{
				internal_snapFound = snap;
			}
			//Debug.Log("Current angle: " + currentAngle + " Sign: " + currentSign + " | Snap angle: " + snapAngle + " Sign: " + snapSign);
		}
		if (internal_snapFound != null)
		{
			_snapped = true;
			return internal_snapFound.transform.position - _startPos;
		}
		_snapped = false;
		return _direction;
	}
}
