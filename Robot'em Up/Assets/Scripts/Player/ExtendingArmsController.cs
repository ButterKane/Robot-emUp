using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

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
	[Separator("General Settings")]
	public ExtendingArmsAimType aimType;
	public Transform startTransform;
	public Transform armTransform;
	public float forwardSpeed;
	public float retractionSpeed;
	public float maxDistance;
	public float freezeDuration;
	public float dragSpeed;
	public float maxDistanceFromWall;
	public float armRadius;
	public float cooldown = 0.1f;
	[ConditionalField(nameof(aimType), false, ExtendingArmsAimType.TwinStick)] public GameObject directionIndicator;

	[Separator("Renderer Settings")]
	public Color armColor;
	public float armWidth;
	public Material armMaterial;

	[Separator("FX")]
	public GameObject FX_retraction;
	public GameObject FX_hit;
	public GameObject FX_extension;

	private Vector3 throwDirection;
	private GameObject throwDirectionIndicator;
	private LineRenderer lineRenderer;
	private Coroutine armExtensionCoroutine;
	private PawnController pawnController;
	private Vector3 currentEndPosition;
	private Transform secondDirectionIndicator;
	private SpriteRenderer secondDirectionIndicatorSprite;
	private float currentCD;

	private ArmState armState;

	private void Awake ()
	{
		pawnController = GetComponent<PawnController>();
		InstantiateLineRenderer();
		InstantiateThrowDirectionIndicator();
		if (aimType != ExtendingArmsAimType.ForwardAiming)
		{
			throwDirectionIndicator.SetActive(true);
		}
		armState = ArmState.Retracted;
	}
	private void Update ()
	{
		UpdateArm();
		if (currentCD > 0 && armState == ArmState.Retracted)
		{
			currentCD -= Time.deltaTime;
		}
	}

	public void SetThrowDirection(Vector3 _direction)
	{
		RaycastHit hit;
		if (Physics.Raycast(startTransform.position, throwDirection, out hit, maxDistance))
		{
			currentEndPosition = hit.point;
			secondDirectionIndicatorSprite.color = Color.green;
		} else
		{
			currentEndPosition = startTransform.position + (throwDirection * maxDistance);
			secondDirectionIndicatorSprite.color = Color.red;
		}
		if (aimType != ExtendingArmsAimType.TwinStick) { return; }
		if (!throwDirectionIndicator.activeSelf) { throwDirectionIndicator.SetActive(true); }
		Vector3 flattedDirection = SwissArmyKnife.GetFlattedDownPosition(_direction, Vector3.zero).normalized;
		throwDirectionIndicator.transform.forward = flattedDirection;
		throwDirection = flattedDirection;

		//Update second indicator position
		secondDirectionIndicator.transform.localPosition = new Vector3(0, 0, Vector3.Distance(startTransform.position, currentEndPosition));
	}

	public void DisableThrowDirectionIndicator()
	{
		throwDirectionIndicator.SetActive(false);
	}

	public void ExtendArm()
	{
		if (armState != ArmState.Retracted || currentCD > 0) { return; }
		switch (aimType)
		{
			case ExtendingArmsAimType.ForwardAiming:
				throwDirection = transform.forward;
				break;
			case ExtendingArmsAimType.TwinStick:
				if (!throwDirectionIndicator.activeSelf) { return; }
				transform.forward = throwDirection;
				pawnController.SetLookInput(throwDirection);
				break;
			default:
				break;
		}
		SetThrowDirection(throwDirection);
		armExtensionCoroutine = StartCoroutine(ExtendArm_C(throwDirection));
	}

	void UpdateArm()
	{
		if (armState != ArmState.Retracted)
		{
			lineRenderer.positionCount = 2;
			lineRenderer.SetPosition(0, startTransform.position);
			lineRenderer.SetPosition(1, armTransform.position);
		} else
		{
			lineRenderer.positionCount = 0;
		}
	}

	void ChangeState(ArmState _newState)
	{
		switch (_newState)
		{
			case ArmState.Extended:
				FXManager.InstantiateFX(FX_hit, armTransform.position, false, -throwDirection, Vector3.one * 3);
				break;
			case ArmState.Retracted:
				break;
			case ArmState.Extending:
				FXManager.InstantiateFX(FX_extension, startTransform.position, false, throwDirection, Vector3.one * 3);
				break;
			case ArmState.Retracting:
				break;
			default:
				break;
		}
		armState = _newState;
	}

	private void InstantiateThrowDirectionIndicator ()
	{
		throwDirectionIndicator = Instantiate(directionIndicator);
		throwDirectionIndicator.name = "ThrowDirectionIndicator";
		throwDirectionIndicator.transform.SetParent(this.transform);
		throwDirectionIndicator.transform.localPosition = Vector3.zero;
		throwDirectionIndicator.transform.localScale = Vector3.one;
		throwDirectionIndicator.transform.localRotation = Quaternion.identity;
		secondDirectionIndicator = throwDirectionIndicator.transform.Find("SecondIndicator");
		secondDirectionIndicatorSprite = secondDirectionIndicator.GetComponentInChildren<SpriteRenderer>();
		secondDirectionIndicator.transform.localPosition = new Vector3(0, 0, maxDistance);
	}

	private void InstantiateLineRenderer()
	{
		GameObject lineRendererObj = new GameObject();
		lineRendererObj.name = "ExtendingArmsRenderer";
		lineRendererObj.transform.SetParent(this.transform);
		lineRenderer = lineRendererObj.AddComponent<LineRenderer>();
		lineRenderer.startWidth = armWidth;
		lineRenderer.endWidth = armWidth;
		lineRenderer.startColor = armColor;
		lineRenderer.endColor = armColor;
		lineRenderer.material = armMaterial;
	}

	private bool TryToAttachArm()
	{
		Collider[] colliderFound = Physics.OverlapSphere(armTransform.position, armRadius, LayerMask.GetMask("Environment"));
		if (colliderFound.Length > 0)
		{
			armTransform.SetParent(colliderFound[0].transform, true);
			armTransform.position = colliderFound[0].ClosestPointOnBounds(armTransform.position);
			return true;
		}
		return false;
	}

	IEnumerator ExtendArm_C(Vector3 _direction)
	{
		ChangeState(ArmState.Extending);
		Vector3 startPosition = startTransform.position;
		Vector3 endPosition = currentEndPosition;
		armTransform.SetParent(null, true);
		for (float i = 0; i < 1; i += Time.deltaTime * forwardSpeed / Vector3.Distance(startPosition, endPosition))
		{
			armTransform.position = Vector3.Lerp(startPosition, endPosition, i / 1f);
			armTransform.transform.forward = endPosition - armTransform.position;
			if (TryToAttachArm())
			{
				ChangeState(ArmState.Extended);
				StartCoroutine(RetractArm_C());
			}
			yield return new WaitForEndOfFrame();
		}
		armTransform.position = endPosition;
		if (TryToAttachArm())
		{
			StartCoroutine(RetractArm_C());
		}
		StartCoroutine(RetractArm_C());
	}

	IEnumerator RetractArm_C()
	{
		if (armExtensionCoroutine != null) { StopCoroutine(armExtensionCoroutine); }
		ChangeState(ArmState.Retracting);

		if (armTransform.parent != null) //Something got grabbed
		{
			Vector3 direction = (armTransform.position - pawnController.transform.position).normalized;
			Vector3 startPosition = pawnController.transform.position;
			Vector3 endPosition = armTransform.position - (direction * maxDistanceFromWall);
			if (Vector3.Distance(startPosition, endPosition) <= 3) { CancelRetraction(); StopAllCoroutines(); }
			yield return new WaitForSeconds(freezeDuration);
			pawnController.Freeze();

			for (float i = 0; i < 1f; i+= Time.deltaTime * dragSpeed / Vector3.Distance(startPosition, endPosition))
			{
				yield return new WaitForEndOfFrame();
				pawnController.transform.position = Vector3.Lerp(startPosition, endPosition, i / 1f);
				if (Physics.Raycast(pawnController.transform.position, direction.normalized, 1f, LayerMask.GetMask("Environment")))
				{
					Debug.Log("Cancelled grab");
					CancelRetraction();
					StopAllCoroutines();
				}
			}
			pawnController.UnFreeze();
			pawnController.transform.position = endPosition;
		} else //Nothing got grabbed
		{
			Vector3 startPosition = armTransform.position;
			for (float i = 0; i < 1; i += Time.deltaTime * retractionSpeed / Vector3.Distance(startPosition, startTransform.position))
			{
				yield return new WaitForEndOfFrame();
				armTransform.position = Vector3.Lerp(startPosition, startTransform.position, i / 1f);
				armTransform.transform.forward = startTransform.position - startPosition;
			}
		}
		CancelRetraction();
	}

	void CancelRetraction()
	{
		pawnController.UnFreeze();
		armTransform.SetParent(startTransform, true);
		armTransform.localPosition = Vector3.zero;
		armTransform.localRotation = Quaternion.identity;
		armTransform.localScale = Vector3.one;
		currentCD = cooldown;
		ChangeState(ArmState.Retracted);
	}
}
