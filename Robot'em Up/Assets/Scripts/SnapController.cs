using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SnapType
{
	Pass
}
public class SnapController : MonoBehaviour
{
	//Type = What do you want to snap ?
	//Object = Which object should be snapped 
	//Angle Treshold = How sensitive is the snap (In degrees)
	//Duration = For how long the object must be snappable ? Negative value means infinite (In sec)
	public static void SetSnappable(SnapType _type, GameObject _object, float _angleTreshold, float _duration = -1)
	{
		//Add snappable script to a gameObject
		Snap newSnap = _object.AddComponent<Snap>();
	}
}
