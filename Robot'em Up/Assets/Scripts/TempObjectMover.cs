using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempObjectMover : MonoBehaviour
{
	public float moveSpeed;
	public float heightOffset;
	public GameObject target;
    void Update()
    {
		Vector3 wantedPosition = transform.position + (transform.forward * Time.deltaTime * moveSpeed);
		RaycastHit hit;
		if (Physics.Raycast(wantedPosition, Vector3.down, out hit, 10f, LayerMask.GetMask("Environment")))
		{
			wantedPosition.y = hit.point.y + heightOffset;
		}

		transform.position = wantedPosition;
		//transform.LookAt(target.transform);
    }
}
