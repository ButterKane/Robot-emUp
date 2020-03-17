using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossLeg : MonoBehaviour, IHitable
{
	public BossBehaviour boss;

	[SerializeField] private bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
	[SerializeField] private float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }

	[Header("Canon settings")]
	public Transform canonVisuals;
	public Transform canonTube;
	[HideInInspector] public float minDelayBetweenBullets;
	[HideInInspector] public float maxDelayBetweenBullets;
	[HideInInspector] public float canonMaxAngle;
	[HideInInspector] public float canonRotationSpeed;
	[HideInInspector] public float maxHP;
	[HideInInspector] public float perfectReceptionDamageMultiplier = 0.25f;
	public float healthBarHeight = 1f;
	public float canonLength;
	public Transform protectionPlateVisuals;

	private ProceduralSpiderLegAnimator legAnimator;
	private float bulletCooldown;
	private bool canonEnabled;
	private Vector3 canonRetractedPosition;
	private Vector3 canonDeployedPosition;
	private float canonAngleLerpValue;
	private Transform healthBar;
	private Image healthBarFill;
	private bool destroyable;
	private bool destroyed;
	private float currentHP;
	private Transform weakPointRedDot;
	private Transform explosionFX;

	private Quaternion protectionPlateInitialRotation;
	private Vector3 protectionPlateInitialPosition;
	private Transform protectionPlateInitialParent;

	private BossSettings bossDatas;

	public void OnHit ( BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default )
	{
		if (destroyed) { return; }
		FeedbackManager.SendFeedback("event.BossLegHit", this);
		legAnimator.StartCoroutine(legAnimator.BumpLeg_C());
		if (_source == DamageSource.PerfectReceptionExplosion)
		{
			_damages = _damages *perfectReceptionDamageMultiplier;
		}
		if (destroyable)
		{
			currentHP -= _damages;
			currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
			if (currentHP <= 0)
			{
				Dismantle();
			}
		} else
		{
			boss.Damage(_damages);
		}
	}


	void Awake()
	{
		bossDatas = BossSettings.GetDatas();
		weakPointRedDot = protectionPlateVisuals.transform.Find("WeakPointRed");
		weakPointRedDot.gameObject.SetActive(false);
		legAnimator = GetComponentInParent<ProceduralSpiderLegAnimator>();
		canonRetractedPosition = canonVisuals.transform.localPosition;
		canonDeployedPosition = canonRetractedPosition - new Vector3(canonLength,0,0);
		canonEnabled = false;
		protectionPlateInitialParent = protectionPlateVisuals.transform.parent;
		protectionPlateInitialPosition = protectionPlateVisuals.transform.localPosition;
		protectionPlateInitialRotation = protectionPlateVisuals.transform.localRotation;
	}

	private void Update ()
	{
		if (canonEnabled && canonVisuals != null && !destroyed)
		{
			canonAngleLerpValue = Mathf.PingPong(Time.time, 1f);
			float wantedAngle = Mathf.Lerp(-canonMaxAngle, canonMaxAngle, canonAngleLerpValue);
			Quaternion wantedRotation = Quaternion.Euler(new Vector3(0, -90 + wantedAngle, 0));
			if (bulletCooldown <= 0)
			{
				canonTube.transform.localScale = Vector3.one * bossDatas.bulletStormSettings.canonScaleMultiplierOnShoot;
				canonVisuals.transform.localRotation = wantedRotation;
				GameObject bullet = Instantiate(Resources.Load<GameObject>("EnemyResource/BossResource/BulletStormPrefab"));
				bullet.transform.position = canonVisuals.transform.position + canonVisuals.transform.forward * 2f;
				bullet.transform.rotation = Quaternion.LookRotation(canonVisuals.forward + AddNoiseOnAngle(0, bossDatas.bulletStormSettings.bulletRandomness));
				bulletCooldown = Random.Range(minDelayBetweenBullets, maxDelayBetweenBullets);
				bullet.GetComponent<BossBullet>().Init(bossDatas);
			} else
			{
				canonTube.transform.localScale = Vector3.Lerp(canonTube.transform.localScale, Vector3.one, Time.deltaTime * bossDatas.bulletStormSettings.canonScaleRecoverSpeed);
				bulletCooldown -= Time.deltaTime;
			}
		}
		if (destroyable)
		{
			UpdateHealthBar();
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

	public void SetDestructible()
	{
		currentHP = maxHP;
		weakPointRedDot.gameObject.SetActive(true);

		//Generates HP bar
		healthBar = Instantiate(Resources.Load<GameObject>("EnemyResource/BossResource/BossLegHealthBar")).transform;
		healthBar.transform.SetParent(GameManager.mainCanvas.transform);
		healthBarFill = healthBar.transform.Find("Fill").GetComponent<Image>();
		healthBarFill.fillAmount = 1;

		destroyable = true;
	}

	private void UpdateHealthBar()
	{
		healthBar.transform.position = GameManager.mainCamera.WorldToScreenPoint(transform.position) + new Vector3(0, healthBarHeight, 0);
		healthBarFill.fillAmount = currentHP / maxHP;
	}

	private void DestroyHealthBar()
	{
		if (healthBar != null)
		{
			Destroy(healthBar.gameObject);
		}
	}

	public void Dismantle()
	{
		explosionFX = FeedbackManager.SendFeedback("event.BossLegDestroyed", this).GetVFX().transform;
		destroyable = false;
		destroyed = true;
		lockable = false;
		DestroyHealthBar();
		canonVisuals.gameObject.SetActive(false);
		//Destroy(canonVisuals.gameObject);
		weakPointRedDot.gameObject.SetActive(false);
		StartCoroutine(DismantleProtectionPlate_C());
	}

	public void Reconstruct()
	{
		destroyed = false;
		lockable = true;
		canonVisuals.gameObject.SetActive(true);
		Destroy(protectionPlateVisuals.GetComponent<Rigidbody>());
		protectionPlateVisuals.gameObject.SetActive(true);
		protectionPlateVisuals.GetComponent<MeshCollider>().isTrigger = true;
		protectionPlateVisuals.transform.SetParent(protectionPlateInitialParent);
		protectionPlateVisuals.transform.localPosition = protectionPlateInitialPosition;
		protectionPlateVisuals.transform.localRotation = protectionPlateInitialRotation;
	}

	public void RemoveExplosionFX()
	{
		if (explosionFX != null)
		{
			ParticleSystem.EmissionModule em = explosionFX.GetComponent<ParticleSystem>().emission;
			em.enabled = false;
			//Destroy(explosionFX.gameObject);
		}
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
		for (float i = 0; i < bossDatas.bulletStormSettings.canonRetractionDuration; i += Time.deltaTime)
		{
			canonVisuals.transform.localPosition = Vector3.Lerp(canonRetractedPosition, canonDeployedPosition, i / bossDatas.bulletStormSettings.canonRetractionDuration);
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		canonEnabled = true;
	}

	IEnumerator RetractCanon_C()
	{
		canonEnabled = false;
		yield return new WaitForSeconds(1f);
		for (float i = 0; i < bossDatas.bulletStormSettings.canonRetractionDuration; i += Time.deltaTime)
		{
			if (!destroyed)
			{
				canonVisuals.transform.localPosition = Vector3.Lerp(canonDeployedPosition, canonRetractedPosition, i / bossDatas.bulletStormSettings.canonRetractionDuration);
				yield return null;
			}
		}
	}

	IEnumerator DismantleProtectionPlate_C ()
	{
		yield return new WaitForSeconds(0.5f);
		Rigidbody ppvrb = protectionPlateVisuals.gameObject.AddComponent<Rigidbody>();
		protectionPlateVisuals.GetComponent<MeshCollider>().isTrigger = false;
		ppvrb.mass = 50;
		ppvrb.transform.SetParent(null);
		ppvrb.AddExplosionForce(1000, transform.position, 10);
		boss.ActivateWeakPoint();
		yield return new WaitForSeconds(3f);
		protectionPlateVisuals.gameObject.SetActive(false);
	}
}
