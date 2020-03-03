using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTeaBag : MonoBehaviour
{
	public GameObject innerCircle;

	public float previewDuration;
	public float pushForce;
	public float damages;
	public float pushRange;
	public float damageRange;
	public BossBehaviour linkedBoss;

	public void Init(BossBehaviour _linkedBoss, float _previewDuration, float _pushForce, float _damages, float _pushRange, float _damageRange)
	{
		transform.localScale = Vector3.one * _damageRange * 2f;
		previewDuration = _previewDuration;
		pushForce = _pushForce;
		damages = _damages;
		pushRange = _pushRange;
		damageRange = _damageRange;
		linkedBoss = _linkedBoss;
		StartCoroutine(TeaBag_C());
		Destroy(this.gameObject, previewDuration + 0.1f);
	}

	IEnumerator TeaBag_C()
	{
		bool animationTriggered = false;
		for (float i = 0; i < previewDuration; i += Time.deltaTime)
		{
			innerCircle.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, i / previewDuration);
			if (i >= previewDuration - 0.4f && !animationTriggered) //0.4f is the length of the animation before hiting the ground
			{
				animationTriggered = true;
				linkedBoss.animator.SetTrigger("Teabag");
			}
			yield return null;
		}
		FeedbackManager.SendFeedback("event.BossTeabag", this.gameObject);

		foreach (PlayerController p in GameManager.alivePlayers)
		{
			float playerDistance = Vector3.Distance(p.transform.position, transform.position);
			if (playerDistance <= damageRange)
			{
				p.Damage(damages);
			}
			if (playerDistance <= pushRange)
			{
				Vector3 pushDirection = Vector3.Normalize(p.transform.position - transform.position);
				pushDirection.y = 0.1f;
				p.Push(pushDirection, pushForce, 0f);
			}
		}
		Destroy(this.gameObject);
	}
}
