using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPunch : MonoBehaviour
{
	public Collider damageCollider;
	public Collider pushCollider;

	private bool damaging;
	private void Awake ()
	{
		damageCollider = GetComponentInChildren<Collider>();
		StartCoroutine(ActivateColliderAfterDelay());
	}

	IEnumerator ActivateColliderAfterDelay ()
	{
		yield return new WaitForSeconds(3f);
		damaging = true;
		damageCollider.enabled = true;
		FeedbackManager.SendFeedback("event.BossPunchHit", this, damageCollider.transform.position, Vector3.up, Vector3.up);
		yield return new WaitForSeconds(0.1f);
		damaging = false;
		damageCollider.enabled = false;
		pushCollider.enabled = true;
		FeedbackManager.SendFeedback("event.BossPunchPush", this, pushCollider.transform.position, Vector3.up, Vector3.up);
		yield return new WaitForSeconds(0.1f);
		pushCollider.enabled = false;
	}

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "Player")
		{
			Debug.Log("Player entered zone");
			if (damaging)
			{
				other.GetComponent<PlayerController>().Damage(20);
			} else
			{
				Vector3 pushDirection = other.transform.position - transform.position;
				pushDirection.y = 3f;
				pushDirection = pushDirection.normalized;
				//other.GetComponent<PlayerController>().Push(pushDirection, 120, 1);
				other.GetComponent<PlayerController>().BumpMe(10, 0.5f, 0.5f, pushDirection, 0, 0, 0);
			}
		}
	}
}
