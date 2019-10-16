using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
	public static GameObject InstantiateFX(GameObject fx, Vector3 _position, bool _useLocalPosition, Transform _parent = null )
	{
		GameObject newFX = Instantiate(fx);
		if (_parent != null)
		{
			newFX.transform.SetParent(_parent);
		}
		if (_useLocalPosition) {
			newFX.transform.localPosition = _position;
		} else
		{
			newFX.transform.position = _position;
		}
		return newFX;
	}
}
