using DitzelGames.FastIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSpiderLegAnimator : MonoBehaviour
{
	public Transform wantedTransform;
	[HideInInspector] public Vector3 wantedTransformPos;
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
	public bool bumpPawnsOnGrounded;
	public float bumpPawnRadius = 2f;

	private void Awake ()
	{
		IK.Target = null;
		isGrounded = true;
	}

	private void LateUpdate ()
	{
		UpdateLeg();
	}

	public void MoveLeg ()
	{
		moveCoroutine = StartCoroutine(MoveLeg_C());
	}

	public void ToggleProceduralAnimation(bool _state)
	{
		IK.enabled = _state;
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
		Vector3 newWantedPos = wantedTransformPos;
		if (Physics.Raycast(IK.transform.position + (Vector3.up * 4) + forwardOffset, Vector3.down, out hit, 20f, LayerMask.GetMask("Environment")))
		{
			newWantedPos.y = hit.point.y;
		}
		wantedTransform.position = newWantedPos;
	}

	public IEnumerator MoveLeg_C ()
	{
		isGrounded = false;
		for (float i = 0; i < 1; i+= Time.deltaTime * (legSpeed * Random.Range(0.8f, 1.2f)))
		{
			if (wantedTransform != null && IK.Target != null && IK.Target.transform != null)
			{
				Vector3 newPosition = Vector3.Lerp(IK.Target.transform.position, wantedTransform.transform.position + forwardOffset, i / 1f);
				float yPos = Mathf.Lerp(IK.Target.transform.position.y, wantedTransform.transform.position.y, i / 1f);
				yPos += heightCurve.Evaluate(i / 1f) * height;
				newPosition.y = yPos;
				IK.Target.transform.position = newPosition;
				yield return null;
			}
		}
		if (IK.Target != null)
		{
			IK.Target.transform.position = wantedTransform.transform.position + forwardOffset;
		}
		isGrounded = true;
		if (eventOnStep != "")
		{
			FeedbackManager.SendFeedback(eventOnStep, this);
		}
		if (bumpPawnsOnGrounded)
		{
			List<PawnController> hitPawns = new List<PawnController>();
			foreach (Collider c in Physics.OverlapSphere(IK.Target.transform.position, bumpPawnRadius))
			{
				PawnController pawnController = c.GetComponentInParent<PawnController>();
				if (pawnController != null && !hitPawns.Contains(pawnController))
				{
					pawnController.Push(PushType.Light, pawnController.transform.position - IK.Target.transform.position, PushForce.Force1);
					hitPawns.Add(pawnController);
				}
			}
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
			for (float i = 0; i < Vector3.Distance(IK.transform.position, hit.point) / 5f; i += Time.deltaTime * 5f)
			{
				IK.Target.transform.position = Vector3.Lerp(startFallPosition, endFallPosition, i / (Vector3.Distance(IK.transform.position, hit.point)) / 5f);
				yield return null;
			}
		}
		isGrounded = true;
	}
}
