using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
	public static GameObject InstantiateFX(GameObject _fx, Vector3 _position, bool _useLocalPosition, Vector3 _direction, Vector3 _sizeMultiplier, Transform _parent = null )
	{
		if (_fx == null)
		{
			Debug.Log("Warning: No FX assigned");
			return null;
		}
		GameObject internal_newFX = Instantiate(_fx);
		if (_parent != null)
		{
			internal_newFX.transform.SetParent(_parent);
		}
		if (_useLocalPosition) {
			internal_newFX.transform.localPosition = _position;
		} else
		{
			internal_newFX.transform.position = _position;
		}
		if (_direction != Vector3.zero)
		{
			internal_newFX.transform.forward = _direction;
		}
		internal_newFX.transform.localScale = new Vector3(internal_newFX.transform.localScale.x * _sizeMultiplier.x, internal_newFX.transform.localScale.y * _sizeMultiplier.y, internal_newFX.transform.localScale.z * _sizeMultiplier.z);
		return internal_newFX;
	}
}
