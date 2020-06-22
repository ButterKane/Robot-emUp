using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLaserGenerator : MonoBehaviour
{
	BossSettings bossDatas;
	private Transform attachedTransform;
	private Vector3 localPosition;
	private Transform forwardTransform;
	private void Awake ()
	{
		bossDatas = BossSettings.GetDatas();
		StartCoroutine(DestroyAfterDelay_C(bossDatas.laserSettings.duration));
	}

	public void FollowTransform(Transform _transform)
	{
		forwardTransform = _transform;
	}
	public void AttachToTransform(Transform _transform)
	{
		attachedTransform = _transform;
		localPosition = transform.localPosition;
	}
	private void Update ()
	{
		if (attachedTransform != null)
		{
			transform.position = attachedTransform.position + localPosition;
		}
		if (forwardTransform != null)
		{
			transform.forward = forwardTransform.forward;
		}
		//transform.Rotate(Vector3.up, Time.deltaTime * bossDatas.laserSettings.rotationSpeed);
	}

	IEnumerator DestroyAfterDelay_C(float _duration)
	{
		yield return new WaitForSeconds(_duration);
		Destroy(this.gameObject);
	}
}
