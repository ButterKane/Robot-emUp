using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPunch : MonoBehaviour
{
	public Collider damageCollider;
	public Collider pushCollider;

	public SpriteRenderer punchOutline;
	public SpriteRenderer punchFill;

	public Color fillColorCharging;
	public Color fillColorHit;

	public float punchChargeDuration = 2f;
	public AnimationCurve punchChargeSpeedCurve;
	public float punchDamages = 5f;
	public float punchPushForce = 10f;
	public float punchPushHeight = 3f;

	private bool damaging;
	private void Awake ()
	{
		damageCollider = GetComponentInChildren<Collider>();
		punchOutline.enabled = false;
		punchFill.enabled = false;
		StartPunch();
	}

	public void StartPunch ()
	{
		StartCoroutine(Punch_C());
	}

	IEnumerator Punch_C ()
	{
		punchOutline.enabled = true;
		punchFill.enabled = true;
		punchFill.color = fillColorCharging;
		Vector3 fillStartPosition = punchFill.transform.localPosition;
		Color newFillColor = punchFill.color;
		for (float i = 0; i < punchChargeDuration; i+=Time.deltaTime)
		{
			punchFill.transform.localPosition = Vector3.Lerp(fillStartPosition, Vector3.zero, punchChargeSpeedCurve.Evaluate(i / punchChargeDuration));
			newFillColor.a = punchChargeSpeedCurve.Evaluate(i / punchChargeDuration);
			punchFill.color = newFillColor;
			yield return null;
		}
		punchFill.transform.localPosition = Vector3.zero;
		newFillColor.a = 1f;
		damaging = true;
		damageCollider.enabled = true;
		FeedbackManager.SendFeedback("event.BossPunchHit", this, damageCollider.transform.position, Vector3.up, Vector3.up);
		for (float i = 0; i < 0.1f; i+=Time.deltaTime)
		{
			punchFill.color = Color.Lerp(newFillColor, fillColorHit, i/0.1f);
			yield return null;
		}
		damaging = false;
		damageCollider.enabled = false;
		pushCollider.enabled = true;
		FeedbackManager.SendFeedback("event.BossPunchPush", this, pushCollider.transform.position, Vector3.up, Vector3.up);
		yield return new WaitForSeconds(0.1f);
		pushCollider.enabled = false;
		punchOutline.enabled = false;
		punchFill.enabled = false;
	}

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "Player")
		{
			Debug.Log("Player entered zone");
			if (damaging)
			{
				other.GetComponent<PlayerController>().Damage(punchDamages);
			} else
			{
				Vector3 pushDirection = other.transform.position - transform.position;
				pushDirection.y = punchPushHeight;
				pushDirection = pushDirection.normalized;
				other.GetComponent<PlayerController>().BumpMe(punchPushForce, 0.5f, 0.5f, pushDirection, 0, 0, 0);
			}
		}
	}
}
