using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLaserGenerator : MonoBehaviour
{
	BossSettings bossDatas;
	private void Awake ()
	{
		bossDatas = BossSettings.GetDatas();
		StartCoroutine(DestroyAfterDelay_C(bossDatas.laserSettings.duration));
	}

	private void Update ()
	{
		transform.Rotate(Vector3.up, Time.deltaTime * bossDatas.laserSettings.rotationSpeed);
	}

	IEnumerator DestroyAfterDelay_C(float _duration)
	{
		yield return new WaitForSeconds(_duration);
		Destroy(this.gameObject);
	}
}
