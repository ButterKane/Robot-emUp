using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum SpawnerType { Air, Underground, Ground }
public class Spawner : MonoBehaviour
{
	public SpawnerType type;
	public Transform startPosition;
	public float delayBeforeBeingFree = 3;

	public float zonePreviewDuration = 3;
	public float spawnDuration = 2;
	public float jumpDistance = 2;
	public AnimationCurve horizontalLerpCurve;
	public AnimationCurve rotationLerpCurve;
	public AnimationCurve verticalLerpCurve;
	public float delayBeforeActivation = 1;

	public Vector3 endPosition;
	public SpriteRenderer endPositionVisualizer;
	public LineRenderer spawnVisualizer;

	[ExecuteInEditMode]
	public void RecalculateEndspawnLocation ()
	{
		if (endPositionVisualizer == null)
		{
			//Generates spawn vizualiser
			GameObject spawnPositionVisualizer = new GameObject();
			spawnPositionVisualizer.name = "EndPositionVizualiser";
			spawnPositionVisualizer.transform.SetParent(transform);
			endPositionVisualizer = spawnPositionVisualizer.AddComponent<SpriteRenderer>();
			endPositionVisualizer.sprite = Resources.Load<Sprite>("ArenaResource/Spawners/spawnPositionVisualizer");
			spawnPositionVisualizer.transform.localScale = Vector3.one * 0.2f;
			spawnPositionVisualizer.transform.localPosition = Vector3.zero;
			spawnPositionVisualizer.transform.rotation = Quaternion.LookRotation(Vector3.up);
		}
		if (spawnVisualizer == null)
		{
			spawnVisualizer = transform.gameObject.AddComponent<LineRenderer>();
			spawnVisualizer.material = Resources.Load<Material>("ArenaResource/Spawners/spawnVisualizerMaterial");
		}

		if (startPosition == null)
		{
			Debug.LogWarning("Spawner doesn't have a start position, please add one");
			return;
		}
		RaycastHit hit;
		LayerMask layerMask = LayerMask.GetMask("Environment");
		switch (type)
		{
			case SpawnerType.Air:
				if (Physics.Raycast(startPosition.position, Vector3.down, out hit, Mathf.Infinity, layerMask))
				{
					endPosition = hit.point;
				}
				spawnVisualizer.positionCount = 2;
				spawnVisualizer.SetPosition(0, startPosition.position);
				spawnVisualizer.SetPosition(1, endPosition);
				break;
			case SpawnerType.Ground:
				Vector3 forwardDirection = startPosition.transform.forward;
				forwardDirection.y = 0;
				forwardDirection = forwardDirection.normalized;
				if (Physics.Raycast(startPosition.position + (forwardDirection * jumpDistance), Vector3.down, out hit, Mathf.Infinity, layerMask))
				{
					endPosition = hit.point;
				}
				spawnVisualizer.positionCount = 10;
				for (int i = 0; i < 10; i++)
				{
					Vector3 horizontalPosition = Vector3.Lerp(startPosition.position, endPosition, horizontalLerpCurve.Evaluate((float)i / 10f));
					Vector3 verticalPosition = Vector3.Lerp(startPosition.position, endPosition, verticalLerpCurve.Evaluate((float)i / 10f));
					Vector3 finalPosition = horizontalPosition;
					finalPosition.y = verticalPosition.y;
					spawnVisualizer.SetPosition(i, finalPosition);
				}
				break;
			case SpawnerType.Underground:
				if (Physics.Raycast(startPosition.position + (Vector3.up * 50), Vector3.down, out hit, Mathf.Infinity, layerMask))
				{
					endPosition = hit.point;
				}
				spawnVisualizer.positionCount = 2;
				spawnVisualizer.SetPosition(0, startPosition.position);
				spawnVisualizer.SetPosition(1, endPosition);
				break;
		}
		endPositionVisualizer.transform.position = endPosition + Vector3.up * 0.05f;
	}
}
