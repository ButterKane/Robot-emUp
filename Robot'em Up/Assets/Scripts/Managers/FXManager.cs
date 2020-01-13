using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
	static IEnumerator spawnDelayCoroutine;
	public static GameObject InstantiateFX(GameObject _fx, Vector3 _position, bool _useLocalPosition, Vector3 _direction, Vector3 _sizeMultiplier, Transform _parent = null)
	{
		if (_fx == null)
		{
			Debug.Log("Warning: No FX assigned");
			return null;
		}
		GameObject i_newFX = Instantiate(_fx);
		if (_parent != null)
		{
			i_newFX.transform.SetParent(_parent);
		}
		if (_useLocalPosition)
		{
			i_newFX.transform.localPosition = _position;
		}
		else
		{
			i_newFX.transform.position = _position;
		}
		if (_direction != Vector3.zero)
		{
			i_newFX.transform.forward = _direction;
		}
		i_newFX.transform.localScale = new Vector3(i_newFX.transform.localScale.x * _sizeMultiplier.x, i_newFX.transform.localScale.y * _sizeMultiplier.y, i_newFX.transform.localScale.z * _sizeMultiplier.z);
		return i_newFX;
	}
}
