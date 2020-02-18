using DitzelGames.FastIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSpiderLegAnimator : MonoBehaviour
{
	public Transform wantedTransform;
	public FastIKFabric IK;
	public float height = 0.5f;
	public float maxDistanceToTarget = 0.8f;
	public AnimationCurve heightCurve;
	public AnimationCurve bumpHeightCurve;
	public float bumpDuration = 0.5f;
	public float bumpHeight = 4f;
	public string eventOnStep;
	[HideInInspector] public float legSpeed;
	public bool isRight;
	[HideInInspector] public bool isGrounded;
	public Vector3 forwardOffset;
	private Coroutine moveCoroutine;

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
		moveCoroutine = StartCoroutine(MoveLeg_C());
	}

	public bool ShouldMove ()
	{
		Vector3 IKFlatted = IK.transform.position;
		IKFlatted.y = 0;
		Vector3 wantedTransformFlatted = wantedTransform.position;
		wantedTransformFlatted.y = 0;
		if (Vector3.Distance(IKFlatted, wantedTransformFlatted) > maxDistanceToTarget && isGrounded)
		{
			return true;
		}
		return false;
	}

	public void UpdateLeg()
	{
		RaycastHit hit;
		Vector3 newWantedPos = wantedTransform.position;
		if (Physics.Raycast(IK.transform.position + (Vector3.up * 2) + forwardOffset, Vector3.down, out hit, 20f, LayerMask.GetMask("Environment")))
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
			Vector3 newPosition = Vector3.Lerp(startPosition, wantedTransform.transform.position + forwardOffset, i / 1f);
			float yPos = Mathf.Lerp(startPosition.y, wantedTransform.transform.position.y, i / 1f);
			yPos += heightCurve.Evaluate(i / 1f) * height;
			newPosition.y = yPos;
			IK.Target.transform.position = newPosition;
			yield return null;
		}
		IK.Target.transform.position = wantedTransform.transform.position + forwardOffset;
		isGrounded = true;
		if (eventOnStep != "")
		{
			FeedbackManager.SendFeedback(eventOnStep, this);
		}
	}

	public IEnumerator BumpLeg_C()
	{
		bool coroutineStopped = false;
		if (moveCoroutine != null) {
			coroutineStopped = true;
			StopCoroutine(moveCoroutine);
		}
		isGrounded = false;
		Vector3 startPosition = IK.Target.transform.position;
		for (float i = 0; i < bumpDuration; i += Time.deltaTime * 1f)
		{
			IK.Target.transform.position = startPosition + (Vector3.up * (bumpHeightCurve.Evaluate(i / bumpDuration) * bumpHeight));
			yield return null;
		}
		RaycastHit hit;
		if (coroutineStopped && Physics.Raycast(IK.transform.position + (Vector3.up * 2) + forwardOffset, Vector3.down, out hit, 20f, LayerMask.GetMask("Environment")))
		{
			Vector3 startFallPosition = IK.transform.position;
			Vector3 endFallPosition = startFallPosition;
			endFallPosition.y = hit.point.y;
			for (float i = 0; i < Vector3.Distance(IK.transform.position, hit.point); i += Time.deltaTime * 5f)
			{
				IK.Target.transform.position = Vector3.Lerp(startFallPosition, endFallPosition, i / Vector3.Distance(IK.transform.position, hit.point));
				yield return null;
			}
		}
		isGrounded = true;
	}
}
