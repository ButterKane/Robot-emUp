using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
	private static Camera _camera;
	public bool invert = false;

	private void LateUpdate()
	{
		if (_camera == null) FindCamera();
		if (_camera == null) return;
		if (invert)
		{
			transform.forward = -_camera.transform.forward;
		}
		else
		{
			transform.forward = _camera.transform.forward;
		}
	}

	private void FindCamera()
	{
		_camera = Camera.current;
	}
}

