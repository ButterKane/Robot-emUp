using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PassMode
{
    Bounce, 
    Curve,
    Straight,
    Count
}
public class PassController : MonoBehaviour
{
	[Header("Global Settings")]
	public bool passPreviewInEditor;

	[Header("Pass settings")]
	public Transform startTransform;
	public PassDatas passData;

	private List<Vector3> pathCoordinates;
	[SerializeField] private BallBehaviour ball;

	private void Update ()
	{
		if (passData == null) { return; }
		pathCoordinates = GetPathCoordinates(startTransform.position, transform.forward, passData.maxLength);
		if (passPreviewInEditor)
			PreviewPathInEditor(pathCoordinates);
	}
	public List<Vector3> GetPathCoordinates(Vector3 _startPosition, Vector3 _direction, float _maxLength)
	{
		RaycastHit hit;
		float remainingLength = _maxLength;
		List<Vector3> pathCoordinates = new List<Vector3>();
		pathCoordinates.Add(_startPosition);

		while (remainingLength > 0)
		{
			Vector3 actualPosition = pathCoordinates[pathCoordinates.Count - 1];
			if (Physics.Raycast(actualPosition, _direction, out hit, remainingLength))
			{
				Debug.DrawRay(actualPosition, hit.normal, Color.red);
				_direction = Vector3.Reflect(_direction, hit.normal);
				Debug.DrawRay(hit.point, _direction, Color.blue);
				pathCoordinates.Add(hit.point);
				remainingLength -= Vector3.Distance(actualPosition, hit.point);
			} else
			{
				pathCoordinates.Add(actualPosition + _direction * remainingLength);
				remainingLength = 0;
			}
		}

		return pathCoordinates;
	}

	public void Shoot()
	{
		StartCoroutine(ShootCoreTowardDirection(passData));
	}

	public void Receive ()
	{

	}

	public void TogglePassPreviewDisplay()
	{

	}

	public bool CanShoot()
	{
		if (ball == null)
		{
			return false;
		} else
		{
			return true;
		}
	}
	private void PreviewPathInEditor(List<Vector3> pathCoordinates)
	{
		for (int i = 0; i < pathCoordinates.Count - 1; i++)
		{
			Vector3 actualPosition = pathCoordinates[i];
			Vector3 nextPosition = pathCoordinates[i + 1];
			Vector3 dir = nextPosition - actualPosition;
			Debug.DrawRay(actualPosition, dir, Color.yellow);
		}
	}

	private float GetPathTotalLength(List<Vector3> pathCoordinates)
	{
		float totalLength = 0;
		for (int i = 0; i < pathCoordinates.Count - 1; i++)
		{
			Vector3 actualPosition = pathCoordinates[i];
			Vector3 nextPosition = pathCoordinates[i + 1];
			totalLength += Vector3.Distance(actualPosition, nextPosition);
		}
		return totalLength;
	}

	IEnumerator ShootCoreTowardDirection(PassDatas passData)
	{
		List<Vector3> currentPathCoordinates = pathCoordinates;
		ball.transform.position = startTransform.position;
		ball.DisableGravity();
		ball.DisableCollisions();

		for (int i = 0; i < currentPathCoordinates.Count - 1; i++)
		{
			if (i > passData.maxBounces) { break; }
			Vector3 currentPoint = currentPathCoordinates[i];
			Vector3 nextPoint = currentPathCoordinates[i + 1];
			float distanceToNextPoint = Vector3.Distance(currentPoint, nextPoint);
			for (float y = 0; y <= distanceToNextPoint; y += Time.deltaTime * passData.moveSpeed)
			{
				ball.transform.position = Vector3.Lerp(currentPoint, nextPoint, y / distanceToNextPoint);
				yield return new WaitForEndOfFrame();
			}
		}
		ball.EnableGravity();
		ball.EnableCollisions();
	}
}
