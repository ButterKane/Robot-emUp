using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLeg : MonoBehaviour, IHitable
{
	public BossBehaviour boss;

	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

	[Header("Canon settings")]
	public Transform canonVisuals;
	public Transform canonTube;
	public float minDelayBetweenBullets;
	public float maxDelayBetweenBullets;
	public float canonMaxAngle;
	public float canonRotationSpeed;
	public float canonRetractionDuration;
	public float canonLength;
	public float scaleMultiplierOnShoot;
	public float scaleRecoverySpeed;

	private ProceduralSpiderLegAnimator legAnimator;
	private float bulletCooldown;
	private bool canonEnabled;
	private Vector3 canonRetractedPosition;
	private Vector3 canonDeployedPosition;
	private float canonAngleLerpValue;

	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default )
	{
		FeedbackManager.SendFeedback("event.BossLegHit", this);
		boss.Damage(_damages);
		legAnimator.StartCoroutine(legAnimator.BumpLeg_C());
	}


	void Awake()
	{
		legAnimator = GetComponentInParent<ProceduralSpiderLegAnimator>();
		canonRetractedPosition = canonVisuals.transform.localPosition;
		canonDeployedPosition = canonRetractedPosition - new Vector3(canonLength,0,0);
		canonEnabled = false;
	}

	private void Update ()
	{
		if (canonEnabled)
		{
			canonAngleLerpValue = Mathf.PingPong(Time.time, 1f);
			float wantedAngle = Mathf.Lerp(-canonMaxAngle, canonMaxAngle, canonAngleLerpValue);
			Quaternion wantedRotation = Quaternion.Euler(new Vector3(0, -90 + wantedAngle, 0));
			if (bulletCooldown <= 0)
			{
				canonTube.transform.localScale = Vector3.one * scaleMultiplierOnShoot;
				canonVisuals.transform.localRotation = wantedRotation;
				GameObject bullet = Instantiate(Resources.Load<GameObject>("EnemyResource/BossResource/BulletStormPrefab"));
				bullet.transform.position = canonVisuals.transform.position + canonVisuals.transform.forward * 2f;
				bullet.transform.rotation = Quaternion.LookRotation(canonVisuals.forward + AddNoiseOnAngle(0, 30));
				bulletCooldown = Random.Range(minDelayBetweenBullets, maxDelayBetweenBullets);
			} else
			{
				canonTube.transform.localScale = Vector3.Lerp(canonTube.transform.localScale, Vector3.one, Time.deltaTime * scaleRecoverySpeed);
				bulletCooldown -= Time.deltaTime;
			}
		}
	}
	public void EnableCanon ()
	{
		StartCoroutine(DeployCanon_C());
	}

	public void DisableCanon()
	{
		StartCoroutine(RetractCanon_C());
	}
	Vector3 AddNoiseOnAngle ( float min, float max )
	{
		// Find random angle between min & max inclusive
		float xNoise = Random.Range(min, max);
		float yNoise = 0f;
		float zNoise = 0f;

		// Convert Angle to Vector3
		Vector3 noise = new Vector3(
		  Mathf.Sin(2 * Mathf.PI * xNoise / 360),
		  Mathf.Sin(2 * Mathf.PI * yNoise / 360),
		  Mathf.Sin(2 * Mathf.PI * zNoise / 360)
		);
		return noise;
	}

	IEnumerator DeployCanon_C ()
	{
		for (float i = 0; i < canonRetractionDuration; i += Time.deltaTime)
		{
			canonVisuals.transform.localPosition = Vector3.Lerp(canonRetractedPosition, canonDeployedPosition, i / canonRetractionDuration);
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		canonEnabled = true;
	}

	IEnumerator RetractCanon_C()
	{
		canonEnabled = false;
		yield return new WaitForSeconds(1f);
		for (float i = 0; i < canonRetractionDuration; i+= Time.deltaTime)
		{
			canonVisuals.transform.localPosition = Vector3.Lerp(canonDeployedPosition, canonRetractedPosition, i / canonRetractionDuration);
			yield return null;
		}
	}

}
