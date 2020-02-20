using DitzelGames.FastIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSpiderLegAnimator : MonoBehaviour
{
	public Transform wantedTransform;
	public FastIKFabric IK;
	public float height = 0.5f;
	public AnimationCurve heightCurve;
	public float legSpeed;
	[HideInInspector] public bool canMove;
	public bool isRight;
	[HideInInspector] public bool isGrounded;

	private void Awake ()
	{
		IK.Target = null;
		isGrounded = true;
	}

	private void Update ()
	{
		UpdateLeg();
	}

	public void MoveLeg ()
	{
		StartCoroutine(MoveLeg_C());
	}

	public bool ShouldMove ()
	{
		Vector3 IKFlatted = IK.transform.position;
		IKFlatted.y = 0;
		Vector3 wantedTransformFlatted = wantedTransform.position;
		wantedTransformFlatted.y = 0;
		if (Vector3.Distance(IKFlatted, wantedTransformFlatted) > 0.8f && isGrounded)
		{
			return true;
		}
		return false;
	}

	public void UpdateLeg()
	{
		RaycastHit hit;
		Vector3 newWantedPos = wantedTransform.position;
		if (Physics.Raycast(IK.transform.position + (Vector3.up * 2), Vector3.down, out hit, 10f, LayerMask.GetMask("Environment")))
		{
			newWantedPos.y = hit.point.y;
		}
		wantedTransform.position = newWantedPos;
	}

	public IEnumerator MoveLeg_C ()
	{
		isGrounded = false;
		Vector3 startPosition = IK.Target.transform.position;
		for (float i = 0; i < 1; i+= Time.deltaTime * (legSpeed * Random.Range(0.8f, 1.2f)))
		{
			Vector3 newPosition = Vector3.Lerp(startPosition, wantedTransform.transform.position, i / 1f);
			float yPos = Mathf.Lerp(startPosition.y, wantedTransform.transform.position.y, i / 1f);
			yPos += heightCurve.Evaluate(i / 1f) * height;
			newPosition.y = yPos;
			IK.Target.transform.position = newPosition;
			yield return null;
		}
		IK.Target.transform.position = wantedTransform.transform.position;
		isGrounded = true;
	}
}
