using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BossLaser : MonoBehaviour
{
	public LineRenderer lr;
	public GameObject laserHitEffect;
	private void Update ()
	{
		Debug.Log("Update");
		if (lr == null) { return; }
		float size = 200f;
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, size, LayerMask.GetMask("Environment")))
		{
			size = Vector3.Distance(transform.position, hit.point);
		}
		lr.SetPosition(1, new Vector3(0, 0, size));
		if (laserHitEffect != null)
		{
			laserHitEffect.transform.position = hit.point;
			laserHitEffect.transform.forward = hit.normal;
		}
	}
}
