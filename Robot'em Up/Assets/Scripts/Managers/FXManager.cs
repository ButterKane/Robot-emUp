using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
	public static GameObject InstantiateFX(GameObject fx, Vector3 _position, bool _useLocalPosition, Vector3 _direction, Vector3 _sizeMultiplier, Transform _parent = null )
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
		if (_direction != Vector3.zero)
		{
			newFX.transform.forward = _direction;
		}
		newFX.transform.localScale = new Vector3(newFX.transform.localScale.x * _sizeMultiplier.x, newFX.transform.localScale.y * _sizeMultiplier.y, newFX.transform.localScale.z * _sizeMultiplier.z);
		return newFX;
	}
}
