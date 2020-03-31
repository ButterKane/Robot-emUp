﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using Knife.DeferredDecals;

public enum ExtendingArmsAimType
{
	TwinStick,
	ForwardAiming
}

public enum ArmState
{
	Extending,
	Extended,
	Retracting,
	Retracted
}
public class ExtendingArmsController : MonoBehaviour
{
	private ArmState currentState;
	private PlayerController linkedPlayer;
	[HideInInspector] public bool previewShown;
	private Transform grabbedObject;
	private Transform currentHitDecal;
	private Decal currentHitDecalScript;
	private Vector3 hitDecalMaxScale;
	private Vector3 directionSet;
	private float timeBetweenLastDirectionSet;
	private Transform grabHand;
	private Transform previewHitObject;
	private Animator animator;

	public Transform armTransform;
	[Header("Preview settings")]
	public LineRenderer previewLineRenderer;
	public GameObject previewHitDecal;
	public SpriteRenderer previewDirectionVisualizer;
	public Color grabbablePreviewColor;
	public Color notGrabbablePreviewColor;

	[Header("Settings")]
	public AnimationCurve extensionSpeedCurve;
	public AnimationCurve retractionSpeedCurve;
	public AnimationCurve dashSpeedCurve;
	public float delayBeforeResettingJoystickDirection;
	public LineRenderer lineRenderer;
	public float maxRange;
	public float rangeTolerance;
	public float grabExtentionSpeed;
	public float grabRetractionSpeed;
	public float dashSpeed;
	public float distanceFromHandOnDash = 1;
	public int frameCountBetweenCollisionRecalculationDuringDash = 1;
	public float dashHitboxRadius = 10f;
	public float dashCollisionPushForce = 5f;
	public float minGrabHoldDuration = 1f;

	[Header("Snap settings")]
	public float snapMinAngle = 10f;
	public AnimationCurve distancePriorityScore;
	public AnimationCurve anglePriorityScore;

	public static List<Collider> grabableObjects = new List<Collider>();

	private void Awake ()
	{
		linkedPlayer = GetComponent<PlayerController>();
		GeneratePreviewDecal();
		animator = linkedPlayer.GetComponentInChildren<Animator>();
		GenerateGrabHand();
		TogglePreview(false);
		ChangeState(ArmState.Retracted);
	}

	private void Update ()
	{
		ShowPreview();
		UpdateDirectionBuffer();
		UpdateGrab();
	}

	public void SetDirection(Vector3 _direction)
	{
		directionSet = _direction;
		timeBetweenLastDirectionSet = 0;
	}

	public void TogglePreview(bool _state)
	{
		previewShown = _state;
		if (!_state)
		{
			previewDirectionVisualizer.enabled = false;
			previewLineRenderer.enabled = false;
			currentHitDecal.gameObject.SetActive(false);
		} else
		{
			previewDirectionVisualizer.enabled = true;
			previewLineRenderer.enabled = true;
		}
	}

	public void ExtendArm()
	{
		linkedPlayer.ChangePawnState("GrabThrowing", ExtendArm_C(), RetractArm_C());
	}

	public void RetractArm()
	{
		StartCoroutine(RetractArm_C());
	}

	private void UpdateDirectionBuffer ()
	{
		if (timeBetweenLastDirectionSet < delayBeforeResettingJoystickDirection)
		{
			timeBetweenLastDirectionSet += Time.deltaTime;
		}
		else
		{
			directionSet = Vector3.zero;
		}
	}

	private void UpdateGrab ()
	{
		if (grabHand.gameObject.activeSelf && lineRenderer != null)
		{
			lineRenderer.positionCount = 2;
			lineRenderer.SetPosition(0, armTransform.position);
			lineRenderer.SetPosition(1, grabHand.transform.position);
		}
		else
		{
			lineRenderer.positionCount = 0;
		}
		if (Vector3.Distance(grabHand.position, armTransform.position) >= maxRange + rangeTolerance)
		{
			RetractArm();
		}
	}
	private void ShowPreview ()
	{
		if (!previewShown) { return; }

		Collider snappedCollider = null;
		float bestPriorityScore = 0;
		foreach (Collider c in grabableObjects)
		{
			if (c == linkedPlayer.mainCollider || c == null) { continue; }
			Vector3 closestPoint = c.ClosestPointOnBounds(armTransform.position);
			Vector3 closestDirectionFlat = closestPoint - armTransform.position;
			closestDirectionFlat.y = 0;
			float angle = Vector3.SignedAngle(closestDirectionFlat.normalized, armTransform.forward, Vector3.up);
			if (Mathf.Abs(angle) < snapMinAngle)
			{
				float scoreValue = distancePriorityScore.Evaluate(closestDirectionFlat.magnitude) + anglePriorityScore.Evaluate(angle);
				if (scoreValue > bestPriorityScore)
				{
					bestPriorityScore = scoreValue;
					snappedCollider = c;
				}
			}
		}

		if (snappedCollider != null)
		{
			previewLineRenderer.transform.forward = snappedCollider.transform.position - previewLineRenderer.transform.position;
		}
		else
		{
			previewLineRenderer.transform.forward = transform.forward;
		}


		previewLineRenderer.positionCount = 2;
		Vector3 hitPosition = previewLineRenderer.transform.position;
		RaycastHit hit;
		/*
		if (Physics.Raycast(previewLineRenderer.transform.position, previewLineRenderer.transform.forward, out hit, maxRange, LayerMask.GetMask("PlayerPart")))
		{
			hitPosition = hit.point;
			currentHitDecal.gameObject.SetActive(true);
			currentHitDecal.transform.position = hit.point;
			currentHitDecal.transform.forward = hit.normal;
			previewHitObject = hit.collider.transform;
		}
		*/
		if (Physics.Raycast(previewLineRenderer.transform.position, previewLineRenderer.transform.forward, out hit, maxRange, ~0, QueryTriggerInteraction.Ignore))
		{
			hitPosition = hit.point;
			currentHitDecal.gameObject.SetActive(true);
			currentHitDecal.transform.position = hit.point - (previewLineRenderer.transform.forward * 0.5f);
			currentHitDecal.transform.forward = hit.normal;
			previewHitObject = hit.collider.transform;
		}
		else
		{
			hitPosition = previewLineRenderer.transform.position + previewLineRenderer.transform.forward * maxRange;
			previewHitObject = null;
		}
		previewLineRenderer.SetPosition(0, previewLineRenderer.transform.position);
		previewLineRenderer.SetPosition(1, hitPosition - (previewLineRenderer.transform.position - hitPosition).normalized * 0.5f);
		if (previewHitObject != null)
		{
			if (IsGrabbable(previewHitObject))
			{
				previewLineRenderer.startColor = grabbablePreviewColor;
				previewLineRenderer.endColor = grabbablePreviewColor;
				currentHitDecalScript.InstancedColor = grabbablePreviewColor;
			} else
			{
				previewLineRenderer.startColor = notGrabbablePreviewColor;
				previewLineRenderer.endColor = notGrabbablePreviewColor;
				currentHitDecalScript.InstancedColor = notGrabbablePreviewColor;
			}
		}
	}

	public void UpdateDecalSize(float percent)
	{
		if (currentHitDecal != null)
		{
			currentHitDecalScript.transform.localScale = Vector3.Lerp(Vector3.zero, hitDecalMaxScale, percent);
		}
	}
	private void GeneratePreviewDecal ()
	{
		currentHitDecal = Instantiate(previewHitDecal).transform;
		currentHitDecalScript = currentHitDecal.transform.Find("DecalMin").GetComponent<Decal>();
		hitDecalMaxScale = currentHitDecal.transform.Find("DecalMax").localScale;
		currentHitDecal.gameObject.SetActive(false);
		DontDestroyOnLoad(currentHitDecal.gameObject);
		GameManager.DDOL.Add(currentHitDecal.gameObject);
	}

	private void GenerateGrabHand ()
	{
		grabHand = Instantiate(Resources.Load<GameObject>("PlayerResource/GrabHand")).transform;
		grabHand.transform.SetParent(armTransform);
		grabHand.transform.localPosition = Vector3.zero;
		grabHand.transform.localRotation = Quaternion.identity;
		grabHand.gameObject.SetActive(false);
		DontDestroyOnLoad(grabHand.gameObject);
		GameManager.DDOL.Add(grabHand.gameObject);
	}

	private bool IsGrabbable (Transform t)
	{
		if (t.gameObject.GetComponent<Grabbable>() != null || t.gameObject.layer == LayerMask.NameToLayer("Player") || t.gameObject.layer == LayerMask.NameToLayer("PlayerPart"))
		{
			return true;
		}
		return false;
	}
	private void CheckWhatGotGrabbed ()
	{
		StartCoroutine(CheckWhatGotGrabbed_C());
	}
	private void ChangeState (ArmState _newState)
	{
		switch (_newState)
		{
			case ArmState.Extended:
				break;
			case ArmState.Retracted:
				break;
			case ArmState.Extending:
				break;
			case ArmState.Retracting:
				break;
			default:
				break;
		}
		currentState = _newState;
	}

	IEnumerator RetractWithObject_C()
	{
		ChangeState(ArmState.Retracting);
		PlayerController grabbedPlayer = null;
		if (grabbedObject.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			grabbedPlayer = grabbedObject.GetComponent<PlayerController>();
			grabbedPlayer.Freeze();
			grabbedPlayer.moveState = MoveState.Blocked;
			grabbedPlayer.animator.SetTrigger("GrabDashTrigger");
		}
		grabHand.gameObject.SetActive(true);
		grabHand.transform.SetParent(null, true);
		grabHand.transform.localScale = Vector3.one;
		Vector3 startPosition = grabHand.transform.position;
		grabHand.transform.localRotation = Quaternion.identity;
		float totalDistance = Vector3.Distance(startPosition, armTransform.position);
		List<EnemyBehaviour> enemyHit = new List<EnemyBehaviour>();
		int colliderRecalculationIterationCount = frameCountBetweenCollisionRecalculationDuringDash;
		for (float i = 0; i < totalDistance; i += Time.deltaTime * grabRetractionSpeed)
		{
			grabHand.transform.position = Vector3.Lerp(startPosition, armTransform.position, retractionSpeedCurve.Evaluate(i / totalDistance));
			if (grabbedPlayer != null)
			{
				grabbedPlayer.transform.position = grabHand.transform.position;
				grabbedPlayer.transform.forward = startPosition - armTransform.position;
			} else
			{
				grabbedObject.transform.position = grabHand.transform.position;
			}
			colliderRecalculationIterationCount--;
			if (colliderRecalculationIterationCount <= 0)
			{
				colliderRecalculationIterationCount = frameCountBetweenCollisionRecalculationDuringDash;
				foreach (Collider c in Physics.OverlapSphere(grabbedObject.transform.position, dashHitboxRadius, LayerMask.GetMask("Enemy")))
				{
					EnemyBehaviour enemy = c.GetComponentInParent<EnemyBehaviour>();
					if (!enemyHit.Contains(enemy))
					{
						enemy.BumpMe(c.transform.position - grabbedObject.transform.position, BumpForce.Force1);
						enemyHit.Add(enemy);
					}
				}
			}
			yield return null;
		}
		grabHand.transform.SetParent(armTransform, true);
		grabHand.transform.localRotation = Quaternion.identity;
		grabHand.transform.position = armTransform.position;
		grabHand.transform.localScale = Vector3.one;
		if (grabbedPlayer != null)
		{
			grabbedPlayer.UnFreeze();
			grabbedPlayer.moveState = MoveState.Idle;
			grabbedPlayer.animator.SetTrigger("GrabDashRecover");
		}
		grabbedObject = null;
		ChangeState(ArmState.Retracted);
	}
	IEnumerator DashTowardHand_C()
	{
		linkedPlayer.animator.SetBool("GrapplePulled", true);
		linkedPlayer.Freeze();
		linkedPlayer.moveState = MoveState.Blocked;
		Vector3 startPosition = armTransform.transform.position;
		Vector3 endPosition = grabHand.transform.position;
		float totalDistance = Vector3.Distance(startPosition, endPosition);
		List<EnemyBehaviour> enemyHit = new List<EnemyBehaviour>();
		int colliderRecalculationIterationCount = frameCountBetweenCollisionRecalculationDuringDash;
		for (float i = 0; i < totalDistance - distanceFromHandOnDash; i+= Time.deltaTime * dashSpeed)
		{
			Vector3 armPosition = Vector3.Lerp(startPosition, endPosition, dashSpeedCurve.Evaluate(i / totalDistance));
			linkedPlayer.transform.position = armPosition + (linkedPlayer.transform.position - armTransform.position);
			linkedPlayer.transform.forward = endPosition - startPosition;
			colliderRecalculationIterationCount--;
			if (colliderRecalculationIterationCount <= 0)
			{
				colliderRecalculationIterationCount = frameCountBetweenCollisionRecalculationDuringDash;
				foreach (Collider c in Physics.OverlapSphere(transform.position, dashHitboxRadius, LayerMask.GetMask("Enemy")))
				{
					EnemyBehaviour enemy = c.GetComponentInParent<EnemyBehaviour>();
					if (!enemyHit.Contains(enemy))
					{
						enemy.BumpMe(c.transform.position - transform.position, BumpForce.Force1);
						enemyHit.Add(enemy);
					}
				}
			}
			yield return null;
		}
		linkedPlayer.UnFreeze();
		linkedPlayer.moveState = MoveState.Idle;
		linkedPlayer.animator.SetBool("GrapplePulled", false);
		RetractArm();
	}

	IEnumerator CancelDashTowardHand_C ()
	{
		yield return null;
		linkedPlayer.UnFreeze();
		linkedPlayer.moveState = MoveState.Idle;
		linkedPlayer.animator.SetBool("GrapplePulled", false);
		RetractArm();
	}

	IEnumerator CancelGrabObjectRetraction_C ()
	{
		yield return null;
		linkedPlayer.UnFreeze();
		linkedPlayer.moveState = MoveState.Idle;
		PlayerController grabbedPlayer;
		grabbedPlayer = grabbedObject.GetComponent<PlayerController>();
		if (grabbedPlayer != null)
		{
			grabbedPlayer.UnFreeze();
			grabbedPlayer.moveState = MoveState.Idle;
		}
		RetractArm();
	}

	IEnumerator ExtendArm_C ()
	{
		ChangeState(ArmState.Extending);
		if (animator == null) { Debug.Log("Animator null"); }
		animator.SetTrigger("GrappleLaunchTrigger");
		directionSet = previewLineRenderer.transform.forward;
		grabHand.gameObject.SetActive(true);
		FeedbackManager.SendFeedback("event.GrabExtensionStart", grabHand);
		grabHand.transform.SetParent(null, true);
		grabHand.transform.localScale = Vector3.one;
		Vector3 startPosition = grabHand.transform.position;
		Vector3 hitPosition = startPosition + directionSet.normalized * maxRange;
		Transform hitObject = null;
		RaycastHit hit;
		if (Physics.Raycast(grabHand.transform.position, directionSet, out hit, maxRange, LayerMask.GetMask("PlayerPart")))
		{
			hitPosition = hit.point;
			grabbedObject = hit.collider.transform;
		} else if (Physics.Raycast(grabHand.transform.position, directionSet, out hit, maxRange, ~0, QueryTriggerInteraction.Ignore))
		{
			hitPosition = hit.point;
			grabbedObject = hit.collider.transform;
			Debug.Log("Grabbed object: ");
		}
		float totalDistance = Vector3.Distance(startPosition, hitPosition);
		for (float i = 0; i < totalDistance; i += Time.deltaTime * grabExtentionSpeed)
		{
			grabHand.transform.position = Vector3.Lerp(startPosition, hitPosition, extensionSpeedCurve.Evaluate( i / totalDistance));
			yield return null;
		}
		if (hitObject != null)
		{
			//grabHand.transform.SetParent(hitObject.transform, true); //Must be fixed, causes deformations
		}
		ChangeState(ArmState.Extended);
		grabHand.transform.forward = hit.normal;
		CheckWhatGotGrabbed();
	}

	IEnumerator RetractArm_C ()
	{
		ChangeState(ArmState.Retracting);
		grabbedObject = null;
		grabHand.gameObject.SetActive(true);
		FeedbackManager.SendFeedback("event.GrabRetractionStart", grabHand);
		grabHand.transform.SetParent(null, true);
		grabHand.transform.localScale = Vector3.one;
		Vector3 startPosition = grabHand.transform.position;
		grabHand.transform.localRotation = Quaternion.identity;
		float totalDistance = Vector3.Distance(startPosition, armTransform.position);
		for (float i = 0; i < totalDistance; i += Time.deltaTime * grabRetractionSpeed)
		{
			grabHand.transform.position = Vector3.Lerp(startPosition, armTransform.position, retractionSpeedCurve.Evaluate(i / totalDistance));
			yield return null;
		}
		grabHand.transform.SetParent(armTransform, true);
		grabHand.transform.localRotation = Quaternion.identity;
		grabHand.transform.position = armTransform.position;
		grabHand.transform.localScale = Vector3.one;
		FeedbackManager.SendFeedback("event.GrabRetractionEnd", grabHand);
		ChangeState(ArmState.Retracted);
	}

	IEnumerator CheckWhatGotGrabbed_C ()
	{
		yield return new WaitForEndOfFrame();
		//If grabbable wall: dash
		if (grabbedObject == null)
		{
			Debug.Log("Check found no object");
			FeedbackManager.SendFeedback("event.GrabHitFail", grabHand);
			animator.SetTrigger("GrappleFail");
			RetractArm();
		}
		else if (grabbedObject.gameObject.GetComponent<Grabbable>() != null)
		{
			FeedbackManager.SendFeedback("event.GrabHit", grabHand);
			linkedPlayer.ChangePawnState("GrabDashing", DashTowardHand_C(), CancelDashTowardHand_C());
		}
		//If player: dash player toward me
		/*
		else if (grabbedObject.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			FeedbackManager.SendFeedback("event.GrabHit", grabHand);
			linkedPlayer.ChangePawnState("GrabPulling", RetractWithObject_C(), CancelGrabObjectRetraction_C());
		}
		//If other player part: dash part toward me and collect it
		else if (grabbedObject.gameObject.layer == LayerMask.NameToLayer("PlayerPart"))
		{
			FeedbackManager.SendFeedback("event.GrabHit", grabHand);
			linkedPlayer.ChangePawnState("GrabPulling", RetractWithObject_C(), CancelGrabObjectRetraction_C());
		}
		*/
		//If enemy shield: wait for other grab //SCOPE ++

		//Else: Retract without anything
		else
		{
			Debug.Log("Check fail");
			FeedbackManager.SendFeedback("event.GrabHitFail", grabHand);
			grabbedObject = null;
			animator.SetTrigger("GrappleFail");
			RetractArm();
		}
	}
}
