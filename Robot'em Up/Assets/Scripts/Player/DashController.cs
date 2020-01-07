﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public enum DashState
{
	Dashing,
	None
}
public class DashController : MonoBehaviour
{
	[Separator("General Settings")]
	public float minDistance = 2f;
	public float maxDistance = 3f;
	public float speed = 10.1f;
	public int maxStackAmount = 3;

	public bool unstoppableDash;
	public bool invincibleDuringDash;

	public float useCooldown = 0.2f;
	public float stackCooldown = 8f;

	public AnimationCurve dashFadeCurve;
	public float dashFadeDuration = 0.2f;

	public float UIFadeDuration = 0.5f;
	public float UIFadeDelay = 1.5f;

	[Separator("Clone Settings")]
	public Transform clonedVisuals;
	public float clonePerSec = 10f;
	public float cloneDuration = 0.5f;
	public Material cloneMaterial;

	[Separator("FX")]
	public GameObject dashTrail;
	public GameObject startDashFX;
	public GameObject endDashFX;

	private PawnController linkedPawn;
	private float currentUseCooldown;
	private float currentStackCooldown;
	private int currentStackAmount;
	private GameObject currentDashFX;

	[ReadOnly] public DashState state;
	private void Awake ()
	{
		linkedPawn = GetComponent<PawnController>();
		currentStackAmount = maxStackAmount;
	}

	private void Update ()
	{
		UpdateCooldown();
		UpdateStackAmount();
	}

	void UpdateStackAmount()
	{
		if (currentStackAmount < maxStackAmount)
		{
			currentStackCooldown += Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.dashRecoverSpeedMultiplier);
			if (currentStackCooldown >= stackCooldown)
			{
				currentStackCooldown = 0;
				currentStackAmount += 1;
			}
		}
	}
	public void Dash()
	{
		if (GetComponent<PlayerUI>() != null)
		{
			GetComponent<PlayerUI>().DisplayDashes();
		}
		if (!CanDash()) { return; }
		SoundManager.PlaySound("PlayerDash", transform.position, transform);
		FeedbackManager.SendFeedback("event.OnDash", this);
		Vector3 internal_startPosition = transform.position;
		Vector3 internal_endPosition = transform.position + transform.forward * maxDistance; 
		//Check for min distance & maxDistance
		RaycastHit hit;
		if (Physics.Raycast(linkedPawn.GetCenterPosition(), transform.forward, out hit, maxDistance))
		{
			if (Vector3.Distance(linkedPawn.GetCenterPosition(), hit.point) <= minDistance)
			{
				return; //Cancel dash
			} else
			{
				internal_endPosition = hit.point - (transform.forward * 0.5f);
			}
		}
		internal_endPosition.y = internal_startPosition.y;

		currentStackAmount--;
		StartCoroutine(Dash_C(internal_startPosition, internal_endPosition));
		currentUseCooldown = useCooldown;
	}
	void ChangeState(DashState _newState)
	{
		switch (_newState)
		{
			case DashState.Dashing:
				if (currentDashFX != null) { Destroy(currentDashFX); }
				FXManager.InstantiateFX(startDashFX, transform.position, false, Vector3.forward, Vector3.one);
				currentDashFX = FXManager.InstantiateFX(dashTrail, Vector3.zero, true, Vector3.zero, Vector3.one * 2, transform);
				break;
			case DashState.None:
				if (state == DashState.Dashing)
				{
					if (currentDashFX != null) { AutoDestroyAfterDelay adad = currentDashFX.AddComponent<AutoDestroyAfterDelay>(); adad.delay = 0.5f; }
					FXManager.InstantiateFX(endDashFX, transform.position, false, Vector3.forward, Vector3.one);
				}
				break;
		}
		state = _newState;
	}

	void GenerateClone()
	{
		GameObject internal_clone = Instantiate(clonedVisuals.gameObject);
		Transform[] internal_childTransforms = clonedVisuals.GetComponentsInChildren<Transform>();
		Transform[] internal_cloneChildTransforms = internal_clone.GetComponentsInChildren<Transform>();
		for (int i = 0; i < internal_childTransforms.Length; i++)
		{
			internal_cloneChildTransforms[i].position = internal_childTransforms[i].position;
			internal_cloneChildTransforms[i].localScale = internal_childTransforms[i].localScale;
			internal_cloneChildTransforms[i].rotation = internal_childTransforms[i].rotation;
			var components = internal_cloneChildTransforms[i].GetComponents(typeof(Component));
			foreach (Component comp in components)
			{
				if (comp.GetType() != typeof(MeshRenderer) && comp.GetType() != typeof(Transform) && comp.GetType() != typeof(MeshFilter) && comp.GetType() != typeof(Renderer))
				{
					Destroy(comp);
				}
				if (comp.GetType() == typeof(MeshRenderer))
				{
					MeshRenderer rd = comp.GetComponent<MeshRenderer>();
					for (int y = 0; y < rd.materials.Length; y++)
					{
						rd.material = cloneMaterial;
						rd.materials[y] = cloneMaterial;
					}
				}
			}
		}
		AutoDestroyAfterDelay internal_newComp = internal_clone.AddComponent<AutoDestroyAfterDelay>();
		internal_newComp.delay = cloneDuration;

	}

	public bool CanDash()
	{
		if (currentUseCooldown >= 0 || currentStackAmount <= 0)
		{
			return false;
		}
		return true;
	}

	void UpdateCooldown()
	{
		if (currentUseCooldown >= 0)
		{
			currentUseCooldown -= Time.deltaTime;
		}
	}

	public float GetCurrentStackCooldown()
	{
		return currentStackCooldown;
	}

	public int GetCurrentStackAmount()
	{
		return currentStackAmount;
	}

	IEnumerator Dash_C ( Vector3 _startPosition, Vector3 _endPosition )
	{
		ChangeState(DashState.Dashing);
		if (invincibleDuringDash) { linkedPawn.SetInvincible(true); }
		float internal_cloneCounter = 0;
		for (float i = 0; i < Vector3.Distance(_startPosition, _endPosition); i += Time.deltaTime * speed)
		{
			internal_cloneCounter += Time.deltaTime;
			if (internal_cloneCounter >= 1f/clonePerSec)
			{
				GenerateClone();
				internal_cloneCounter = 0;
			}
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, 0.1f))
			{
				if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Environment"))
				{
					StopAllCoroutines();
				}
			}
			transform.position = Vector3.Lerp(_startPosition, _endPosition, i / Vector3.Distance(_startPosition, _endPosition));
			yield return new WaitForEndOfFrame();
		}
		transform.position = _endPosition;
		GenerateClone();
		ChangeState(DashState.None);
		StartCoroutine(FadePlayerSpeed());
		linkedPawn.SetInvincible(false);
	}

	IEnumerator FadePlayerSpeed()
	{
		for (float i = 0; i < dashFadeDuration; i+=Time.deltaTime)
		{
			linkedPawn.AddSpeedCoef(new SpeedCoef(1 + dashFadeCurve.Evaluate(i / dashFadeDuration) * (speed * 0.015f), Time.deltaTime, SpeedMultiplierReason.Dash, false));
			yield return null;
		}
	}

}
