using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossPunch : MonoBehaviour
{
	public Collider damageCollider;
	public Collider pushCollider;

	public SpriteRenderer punchOutline;
	public MeshRenderer punchFill;

	public Color fillColorCharging;
	public Color fillColorHit;

	public float punchChargeDuration = 2f;
	public AnimationCurve punchChargeSpeedCurve;
	public float punchDamages = 5f;
	public float punchPushForce = 10f;
	public float punchPushHeight = 3f;
	private bool cubeDestroyed = false;

	private bool damaging;
	private void Awake ()
	{
		punchOutline.enabled = false;
		punchFill.enabled = false;
	}

	public void StartPunch ()
	{
		StartCoroutine(Punch_C());
	}

	IEnumerator Punch_C ()
	{
		punchOutline.enabled = true;
		punchFill.enabled = true;
		punchFill.material.SetColor("_Tint",fillColorCharging);
		Color newFillColor = punchFill.material.GetColor("_Tint");
		for (float i = 0; i < punchChargeDuration; i+=Time.deltaTime)
		{
			//punchFill.transform.localPosition = Vector3.Lerp(fillStartPosition, Vector3.zero, punchChargeSpeedCurve.Evaluate(i / punchChargeDuration));
			punchFill.material.SetFloat("_QuadCompletion", i / punchChargeDuration);
			newFillColor.a = punchChargeSpeedCurve.Evaluate(i / punchChargeDuration);
			punchFill.material.SetColor("_Tint", newFillColor);
			yield return null;
		}
		newFillColor.a = 1f;
		damaging = true;
		damageCollider.enabled = true;
		FeedbackManager.SendFeedback("event.BossPunchHit", this, damageCollider.transform.position, Vector3.up, Vector3.up);
		for (float i = 0; i < 0.1f; i+=Time.deltaTime)
		{
			punchFill.material.SetColor("Tint", Color.Lerp(newFillColor, fillColorHit, i / 0.1f));
			yield return null;
		}
		damaging = false;
		damageCollider.enabled = false;
		pushCollider.enabled = true;
		FeedbackManager.SendFeedback("event.BossPunchPush", this, pushCollider.transform.position, transform.forward, Vector3.up);
		yield return new WaitForSeconds(0.1f);
		pushCollider.enabled = false;
		punchOutline.enabled = false;
		punchFill.enabled = false;
	}

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "Player")
		{
			if (damaging)
			{
				other.GetComponent<PlayerController>().Damage(punchDamages);
				Vector3 pushDirection = other.transform.position - transform.position;
				pushDirection.y = punchPushHeight;
				pushDirection = pushDirection.normalized;
				other.GetComponent<PlayerController>().BumpMe(punchPushForce, 0.5f, 0.5f, pushDirection, 0, 0, 0);
			} else
			{
				Vector3 pushDirection = other.transform.position - transform.position;
				pushDirection.y = punchPushHeight;
				pushDirection = pushDirection.normalized;
				other.GetComponent<PlayerController>().BumpMe(punchPushForce, 0.5f, 0.5f, pushDirection, 0, 0, 0);
			}
		} else if (other.tag == "Boss_Destructible" && !cubeDestroyed)
		{
			cubeDestroyed = true;
			StartCoroutine(Destroy_Tile_C(other.transform));
		}
	}

	IEnumerator Destroy_Tile_C(Transform _tile)
	{
		Vector3 startPosition = _tile.position;
		_tile.GetComponent<Collider>().enabled = false;
		yield return null;
		BossTileGenerator.instance.nms.BuildNavMesh();
		for (float i = 0; i < 3f; i+= Time.deltaTime)
		{
			_tile.position = Vector3.Lerp(startPosition, startPosition + Vector3.down * 20f, i / 3f);
			yield return null;
		}
		Destroy(_tile.gameObject);
	}
}
