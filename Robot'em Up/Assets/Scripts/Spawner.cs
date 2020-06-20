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
	public float delayBeforeActivation = 0.25f;
	public bool attachSpawnedObject = false;
	public GameObject enemyToSpawn;

	public Vector3 endPosition;
	public SpriteRenderer endPositionVisualizer;
	public LineRenderer spawnVisualizer;
	float currentDelayBeforeBeingFree;
	private EnemyBehaviour lastSpawnedEnemy;
	private bool spawning;
	private Animator animator;
	private bool opened;


	[ExecuteAlways]
	private void Awake ()
	{
		opened = false;
		animator = GetComponent<Animator>();
		if (endPositionVisualizer != null)
		{
			if (Application.isPlaying)
			{
				endPositionVisualizer.enabled = false;
			} else
			{
				endPositionVisualizer.enabled = true;
			}
		}
		if (spawnVisualizer != null)
		{
			if (Application.isPlaying)
			{
				spawnVisualizer.enabled = false;
			}
			else
			{
				spawnVisualizer.enabled = true;
			}
		}
		RecalculateEndspawnLocation();
	}

	private void Update ()
	{
		if (currentDelayBeforeBeingFree > 0 && !spawning && !opened) { currentDelayBeforeBeingFree -= Time.deltaTime; }
		if (opened)
		{
			if (!IsBlocked() && !spawning)
			{
				opened = false;
				if (animator != null) { animator.SetTrigger("Close"); }
			}
		}
	}

	public bool IsBlocked()
	{
		LayerMask mask = LayerMask.GetMask("Environment");
		mask = ~mask;
		foreach (Collider c in Physics.OverlapSphere(endPosition, 1f, mask))
		{
			EnemyBehaviour enemyFound = c.GetComponent<EnemyBehaviour>();
			if (lastSpawnedEnemy != null && enemyFound == lastSpawnedEnemy)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFree()
	{
		if (currentDelayBeforeBeingFree > 0) { return false; }
		if (lastSpawnedEnemy == null) { return true; }
		if (IsBlocked()) { return false; }
		return true;
	}
	public EnemyBehaviour SpawnEnemy(EnemyData _enemy, bool _addSmallRandomDelay, float _spawnSpeedOverride = -1)
	{
		spawning = true;
		GameObject i_newEnemy = Instantiate(_enemy.prefab).gameObject;
		EnemyBehaviour i_enemyBehaviour = i_newEnemy.GetComponent<EnemyBehaviour>();
		if (attachSpawnedObject)
		{
			i_enemyBehaviour.transform.SetParent(transform);
		}
		if (i_enemyBehaviour == null) { Destroy(i_newEnemy); Debug.LogWarning("Spawner can't instantiate enemy: invalid prefab"); return null; }
		if (i_enemyBehaviour.GetNavMesh() != null) { i_enemyBehaviour.GetNavMesh().enabled = false; }
		lastSpawnedEnemy = i_enemyBehaviour;
		currentDelayBeforeBeingFree = delayBeforeBeingFree;
		i_enemyBehaviour.ChangeState(EnemyState.Spawning);
		if (i_enemyBehaviour.GetNavMesh() != null) { i_enemyBehaviour.GetNavMesh().enabled = false; }
		StartCoroutine(SpawnEnemy_C(i_enemyBehaviour, _addSmallRandomDelay, _spawnSpeedOverride));
		return i_enemyBehaviour;
	}

	public void SpawnEnemy()
	{
        if (IsFree() | type != SpawnerType.Ground)
        {
            EnemyData newEnemyData = new EnemyData();
            newEnemyData.prefab = enemyToSpawn;
            SpawnEnemy(newEnemyData, false);
        }
	}

	public void RetractEnemy(EnemyBehaviour _enemy)
	{
		if (type != SpawnerType.Underground) { return; }
		StartCoroutine(RetractEnemy_C(_enemy));
	}

	IEnumerator RetractEnemy_C(EnemyBehaviour _enemy)
	{
		opened = true;
		if (animator) { animator.SetTrigger("Close"); }
		_enemy.ChangeState(EnemyState.Spawning);
		if (_enemy.GetNavMesh() != null) { _enemy.GetNavMesh().enabled = false; }
		PawnController enemyPawn = (PawnController)_enemy;
		for (float i = 0; i < spawnDuration; i += Time.deltaTime)
		{
			RecalculateEndspawnLocation();
			_enemy.ChangeState(EnemyState.Spawning);
			enemyPawn.transform.position = Vector3.Lerp(endPosition, startPosition.position, verticalLerpCurve.Evaluate(i / spawnDuration));
			yield return null;
		}
		Destroy(_enemy.gameObject);
	}
	IEnumerator SpawnEnemy_C(EnemyBehaviour _enemy, bool _addSmallRandomDelay, float _spawnSpeedOverride)
	{
		switch (type)
		{
			case SpawnerType.Air:
				FeedbackManager.SendFeedback("event.SpawnerAerial", this);
				yield return new WaitForSeconds(0.5f);
				break;
			case SpawnerType.Ground:
				FeedbackManager.SendFeedback("event.SpawnerGround", this);
				break;
			case SpawnerType.Underground:
				FeedbackManager.SendFeedback("event.SpawnerWall", this);
				break;
		}
		if (_spawnSpeedOverride != -1)
		{
			spawnDuration = _spawnSpeedOverride;
		}
		//_enemy.transform.localScale = Vector3.one;
		_enemy.gameObject.SetActive(false);
		_enemy.transform.position = startPosition.position;
		if (_addSmallRandomDelay) { yield return new WaitForSeconds(Random.Range(1.5f, 3f)); }

		opened = true;
		if (animator) { animator.SetTrigger("Open"); }
		if (_spawnSpeedOverride != -1)
		{
			yield return new WaitForSeconds(0.1f);
		}
		_enemy.ChangeState(EnemyState.Spawning);
		if (_enemy.GetNavMesh() != null) { _enemy.GetNavMesh().enabled = false; }
		GameObject explosionVisualizer = default;
		if (type == SpawnerType.Air)
		{
			_enemy.gameObject.SetActive(false);
			explosionVisualizer = Instantiate(Resources.Load<GameObject>("ArenaResource/Spawners/P_ExplosionVisualizer"));
			explosionVisualizer.transform.position = endPosition + Vector3.up * 0.01f;
			explosionVisualizer.transform.rotation = Quaternion.LookRotation(Vector3.up);
			IndianaExplosion ie = explosionVisualizer.GetComponent<IndianaExplosion>();
			ie.myScale = _enemy.spawnImpactRadius * 0.33f;
			ie.waitTimeForExplosion = zonePreviewDuration / 3f;
			ie.Initiate();
			yield return new WaitForSeconds(zonePreviewDuration / 3f);
			_enemy.gameObject.SetActive(true);

		}
		else
		{
			_enemy.gameObject.SetActive(true);
		}
		PawnController enemyPawn = (PawnController)_enemy;
		if (_enemy.GetNavMesh() != null) { _enemy.GetNavMesh().enabled = false; }
		for (float i = 0; i < spawnDuration; i += Time.deltaTime)
		{
			RecalculateEndspawnLocation();
			_enemy.ChangeState(EnemyState.Spawning);
			if (_enemy.GetNavMesh() != null) { _enemy.GetNavMesh().enabled = false; }
			switch (type)
			{
				case SpawnerType.Ground:
					Vector3 i_horizontalPosition = Vector3.Lerp(startPosition.position, endPosition, horizontalLerpCurve.Evaluate(i / spawnDuration));
					Vector3 i_verticalPosition = Vector3.Lerp(startPosition.position, endPosition, verticalLerpCurve.Evaluate(i / spawnDuration));
					Vector3 i_finalPosition = i_horizontalPosition;
					Vector3 i_flatDirection = endPosition - startPosition.position;
					i_flatDirection.y = 0;
					Quaternion i_wantedRotation = Quaternion.Lerp(startPosition.rotation, Quaternion.LookRotation(i_flatDirection), rotationLerpCurve.Evaluate(i / spawnDuration));
					i_finalPosition.y = i_verticalPosition.y;
					enemyPawn.transform.position = i_horizontalPosition;
					enemyPawn.transform.rotation = i_wantedRotation;
					break;
				case SpawnerType.Air:
					enemyPawn.transform.position = Vector3.Lerp(startPosition.position, endPosition, verticalLerpCurve.Evaluate(i / spawnDuration));
					break;
				case SpawnerType.Underground:
					enemyPawn.transform.position = Vector3.Lerp(startPosition.position, endPosition, verticalLerpCurve.Evaluate(i / spawnDuration));
					break;
			}
			yield return null;
		}
		enemyPawn.transform.position = endPosition;
		if (type == SpawnerType.Air)
		{
			if (explosionVisualizer != null && _enemy != null)
			{
				foreach (Collider col in Physics.OverlapSphere(explosionVisualizer.transform.position, _enemy.spawnImpactRadius))
				{
					IHitable hitableTarget = col.GetComponent<IHitable>();
					if (hitableTarget != null)
					{
						hitableTarget.OnHit(default, explosionVisualizer.transform.position - col.transform.position, _enemy, _enemy.spawnImpactDamages, DamageSource.SpawnImpact);
					}
				}
			}
			Destroy(explosionVisualizer);
			FeedbackManager.SendFeedback("event.EnemyGroundImpact", _enemy);
			
		}
		spawning = false;
		_enemy.ChangeState(EnemyState.Spawning);
		if (_enemy.GetNavMesh() != null) { _enemy.GetNavMesh().enabled = false; }
		yield return new WaitForSeconds(delayBeforeActivation);
		if (_enemy.GetNavMesh() != null) { _enemy.GetNavMesh().enabled = true; }
		_enemy.ChangeState(EnemyState.Deploying);
	}

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
			spawnVisualizer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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
				spawnVisualizer.positionCount = 11;
				for (int i = 0; i < 10; i++)
				{
					Vector3 horizontalPosition = Vector3.Lerp(startPosition.position, endPosition, horizontalLerpCurve.Evaluate((float)i / 10f));
					Vector3 verticalPosition = Vector3.Lerp(startPosition.position, endPosition, verticalLerpCurve.Evaluate((float)i / 10f));
					Vector3 finalPosition = horizontalPosition;
					finalPosition.y = verticalPosition.y;
					spawnVisualizer.SetPosition(i, finalPosition);
				}
				spawnVisualizer.SetPosition(10, endPosition);
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
