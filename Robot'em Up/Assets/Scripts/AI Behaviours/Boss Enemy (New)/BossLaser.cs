using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BossLaser : MonoBehaviour
{
	public LineRenderer lr;
	public GameObject laserHitEffect;
	public BoxCollider col;

	private bool enabled = false;
	private float currentMaxLength;
	private bool charging;
	private BossSettings bossDatas;

	private void Awake ()
	{
		bossDatas = BossSettings.GetDatas();
		enabled = false;
		currentMaxLength = 0f;
		lr.startWidth = bossDatas.laserSettings.chargingWidth;
		lr.endWidth = bossDatas.laserSettings.chargingWidth;
		laserHitEffect.gameObject.SetActive(false);
	}
	private void Update ()
	{
		if (lr == null) { return; }
		if (currentMaxLength < bossDatas.laserSettings.maxLength)
		{
			currentMaxLength += Time.deltaTime * bossDatas.laserSettings.maxLengthIncrementationSpeed;
			currentMaxLength = Mathf.Clamp(currentMaxLength, 0, bossDatas.laserSettings.maxLength);
		}
		float size = 200f;
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, size, LayerMask.GetMask("Environment")))
		{
			size = Vector3.Distance(transform.position, hit.point);
		}
		if (currentMaxLength >= size && !charging)
		{
			charging = true;
			StartCoroutine(ChargeLaser_C());
		}
		size = Mathf.Clamp(size, 0, currentMaxLength);
		lr.SetPosition(1, new Vector3(0, 0, size));
		if (laserHitEffect != null)
		{
			laserHitEffect.transform.position = hit.point;
			laserHitEffect.transform.forward = hit.normal;
		}
		if (col != null)
		{
			col.center = new Vector3(0, 0, size / 2f);
			col.size = new Vector3(1, 1, size);
		}
	}

	private void OnTriggerStay ( Collider other )
	{
		if (other.tag == "Player" && enabled)
		{
			PlayerController player = other.GetComponent<PlayerController>();
			player.Damage(Time.deltaTime * bossDatas.laserSettings.damagesPerSecond);
			player.Push(PushType.Light, transform.forward, PushForce.Force1);
		}
	}

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "Player" && enabled)
		{
			PlayerController player = other.GetComponent<PlayerController>();
			player.Damage(bossDatas.laserSettings.damagesOnEnter);
		}
	}

	IEnumerator ChargeLaser_C()
	{
		yield return new WaitForSeconds(bossDatas.laserSettings.chargeDelay);
		for (float i = 0; i < bossDatas.laserSettings.chargeDuration; i+=Time.deltaTime)
		{
			float newWidth = Mathf.Lerp(bossDatas.laserSettings.chargingWidth, bossDatas.laserSettings.defaultWidth, bossDatas.laserSettings.chargeSpeedCurve.Evaluate(i / bossDatas.laserSettings.chargeDuration));
			lr.startWidth = newWidth;
			lr.endWidth = newWidth;
			yield return null;
		}
		lr.startWidth = bossDatas.laserSettings.defaultWidth;
		lr.endWidth = bossDatas.laserSettings.defaultWidth;
		laserHitEffect.gameObject.SetActive(true);
		enabled = true;
	}
}
