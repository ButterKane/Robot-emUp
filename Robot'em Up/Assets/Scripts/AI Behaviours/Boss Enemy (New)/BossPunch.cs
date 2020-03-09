using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MyBox;

public class BossPunch : MonoBehaviour
{
	public bool canDestroyEnviro;
	public bool canDamagePlayers;
	public bool canPushPlayers;

	[ConditionalField("canDestroyEnviro")] public Collider destroyEnviroCollider;
	[ConditionalField("canDamagePlayers")] public Collider damageCollider;
	[ConditionalField("canPushPlayers")] public Collider pushCollider;

	public SpriteRenderer punchOutline;
	public MeshRenderer punchFill;

	public Color fillColorCharging;
	public Color fillColorHit;

	public float punchChargeDuration = 2f;
	public AnimationCurve punchChargeSpeedCurve;
	[ConditionalField("canDamagePlayers")] public float punchDamages = 5f;
	[ConditionalField("canPushPlayers")] public float punchPushForce = 10f;
	[ConditionalField("canPushPlayers")] public float punchPushHeight = 3f;
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
		if (damageCollider != null && canDamagePlayers)
		{
			damaging = true;
			damageCollider.enabled = true;
			FeedbackManager.SendFeedback("event.BossPunchHit", this, damageCollider.transform.position, Vector3.up, Vector3.up);
			yield return null;
			damageCollider.enabled = false;
		}

		if (pushCollider != null && canPushPlayers)
		{
			for (float i = 0; i < 0.1f; i += Time.deltaTime)
			{
				punchFill.material.SetColor("Tint", Color.Lerp(newFillColor, fillColorHit, i / 0.1f));
				yield return null;
			}
			damaging = false;
			pushCollider.enabled = true;
			FeedbackManager.SendFeedback("event.BossPunchPush", this, pushCollider.transform.position, transform.forward, Vector3.up);
			yield return null;
			pushCollider.enabled = false;
		}

		if (destroyEnviroCollider != null && canDestroyEnviro)
		{
			destroyEnviroCollider.enabled = true;
			FeedbackManager.SendFeedback("event.BossHammerHit", this, damageCollider.transform.position, Vector3.up, Vector3.up);
			yield return null;
			destroyEnviroCollider.enabled = false;
		}

		yield return new WaitForSeconds(0.1f);
		punchOutline.enabled = false;
		punchFill.enabled = false;
	}

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "Player")
		{
			if (canDamagePlayers && damageCollider != null && damageCollider.enabled)
			{
				other.GetComponent<PlayerController>().Damage(punchDamages);
				Vector3 pushDirection = other.transform.position - transform.position;
				pushDirection.y = punchPushHeight;
				pushDirection = pushDirection.normalized;
				other.GetComponent<PlayerController>().BumpMe(punchPushForce, 0.5f, 0.5f, pushDirection, 0, 0, 0);
			} else if (canPushPlayers && pushCollider != null && pushCollider.enabled)
			{
				Vector3 pushDirection = other.transform.position - transform.position;
				pushDirection.y = punchPushHeight;
				pushDirection = pushDirection.normalized;
				other.GetComponent<PlayerController>().BumpMe(punchPushForce, 0.5f, 0.5f, pushDirection, 0, 0, 0);
			}
		} else if (other.tag == "Boss_Tile" && !cubeDestroyed && canDestroyEnviro && destroyEnviroCollider != null && destroyEnviroCollider.enabled)
		{
			cubeDestroyed = true;
			other.GetComponentInParent<BossTile>().DestroyTile();
		}
        else if (other.tag == "Boss_Destructible")
        {
            Destroy(other.gameObject);

        }
	}
}
