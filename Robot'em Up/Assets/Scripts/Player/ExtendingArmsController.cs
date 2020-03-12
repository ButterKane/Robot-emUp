using System.Collections;
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
	private bool previewShown;
	private Transform grabbedObject;
	private Transform currentHitDecal;
	private Vector3 directionSet;
	private float timeBetweenLastDirectionSet;

	public Transform armTransform;
	[Header("Preview settings")]
	public LineRenderer previewLineRenderer;
	public GameObject previewHitDecal;
	public SpriteRenderer previewDirectionVisualizer;

	[Header("Settings")]
	public float delayBeforeResettingJoystickDirection;
	public LineRenderer lineRenderer;
	public float maxRange;

	private void Awake ()
	{
		linkedPlayer = GetComponent<PlayerController>();
		GeneratePreviewDecal();
		TogglePreview(false);
	}

	private void Update ()
	{
		ShowPreview();
		UpdateDirectionBuffer();
	}

	private void UpdateDirectionBuffer ()
	{
		if (timeBetweenLastDirectionSet < delayBeforeResettingJoystickDirection) {
			timeBetweenLastDirectionSet += Time.deltaTime;
		} else
		{
			directionSet = Vector3.zero;
		}
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
		if (directionSet != Vector3.zero) {
			Debug.Log("Extending arm");
		}
	}

	public void RetractArm()
	{

	}

	void ShowPreview()
	{
		if (!previewShown) { return; }
		Debug.Log("Updating preview);");
		previewLineRenderer.positionCount = 2;
		Vector3 hitPosition = previewLineRenderer.transform.position;
		RaycastHit hit;
		if (Physics.Raycast(previewLineRenderer.transform.position, previewLineRenderer.transform.forward, out hit, maxRange, LayerMask.GetMask("Environment")))
		{
			hitPosition = hit.point;
			currentHitDecal.gameObject.SetActive(true);
			currentHitDecal.transform.position = hit.point;
			currentHitDecal.transform.forward = hit.normal;
		} else
		{
			currentHitDecal.gameObject.SetActive(false);
		}
		previewLineRenderer.SetPosition(0, previewLineRenderer.transform.position);
		previewLineRenderer.SetPosition(1, hitPosition - (previewLineRenderer.transform.position - hitPosition).normalized * 0.2f);
	}

	void GeneratePreviewDecal ()
	{
		currentHitDecal = Instantiate(previewHitDecal).transform;
		currentHitDecal.gameObject.SetActive(false);
	}

	void ChangeState(ArmState _newState)
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
}
