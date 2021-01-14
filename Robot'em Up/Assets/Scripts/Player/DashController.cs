﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Analytics;

public enum DashState
{
	Dashing,
	None
}
public class DashController : MonoBehaviour
{
	[Separator("General Settings")]
	public float minDistance = 2f; //If the free space in front of the player is less than minDistance, then he can't use the dash
	public float distance = 3f; //Max distance travelled by the player during the dash
	public float duration = 0.2f;
	public AnimationCurve dashSpeedCurve;
	public int upgradeMaxStackAmount = 3;
	public int defaultMaxStackAmount = 2;

	public bool unstoppableDash;
	public float pushForce = 80f;
	public float pushHeight = 0.2f;
	public float dashHitboxSize = 0.7f;

	public float useCooldown = 0.2f;
	public float defaultStackRecoveryDuration = 8f;
	public float minStackRecoverySpeedMultiplier = 1f;
	public float maxStackRecoverySpeedMultiplier = 2f;
	public AnimationCurve dashRecoverySpeedCurve;

	public AnimationCurve dashFadeCurve;
	public float dashFadeDuration = 0.2f;

	public float UIFadeDuration = 0.5f;
	public float UIFadeDelay = 1.5f;

	[Separator("Clone Settings")]
	public Transform clonedVisuals;
	public float clonePerSec = 10f;
	public float cloneDuration = 0.5f;
	public Material cloneMaterial;

	private PawnController linkedPawn;
	private float currentUseCooldown;
	private float currentStackCooldown;
	private int currentStackAmount;
	private GameObject currentDashFX;
	private PlayerUI playerUI;
	[HideInInspector] public int maxStackAmount = 2;
	private bool wallHit = false;

	[ReadOnly] public DashState state;
	private void Awake ()
	{
		linkedPawn = GetComponent<PawnController>();
		playerUI = GetComponent<PlayerUI>();
		maxStackAmount = defaultMaxStackAmount;
		currentStackAmount = maxStackAmount;
	}
	private void Update ()
	{
		UpdateCooldown();
		UpdateStackAmount();
	}

	#region Public functions
	public void Dash ( Vector3 _direction )
	{
		if (!CanDash()) { return; }
		if (playerUI != null)
		{
			playerUI.DisplayDashes();
		}
		wallHit = false;
		Analytics.CustomEvent("PlayerDash", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
		_direction = _direction.normalized;
		Vector3 i_startPosition = transform.position;
		Vector3 i_endPosition = transform.position + _direction * distance;
		//Check for min distance & maxDistance
		RaycastHit hit;
		if (Physics.SphereCast(linkedPawn.GetCenterPosition() - _direction, dashHitboxSize ,_direction, out hit, distance + 1, LayerMask.GetMask("Environment")))
		{
			if (Vector3.Distance(linkedPawn.GetCenterPosition(), hit.point) <= minDistance)
			{
				return; //Cancel dash
			}
			else
			{
				i_endPosition = hit.point - (_direction * 1f);
				wallHit = true;
			}
		}
		i_endPosition.y = i_startPosition.y;

		linkedPawn.ChangePawnState("Dashing", Dash_C(i_startPosition, i_endPosition), StopDash_C());
	}
	public float GetCurrentStackCooldown ()
	{
		return currentStackCooldown;
	}
	public int GetCurrentStackAmount ()
	{
		return currentStackAmount;
	}

    public void RecoverAllStackAmount()
    {
        currentStackAmount = maxStackAmount;
    }

	public void CheckForUpgrades()
	{
		if (AbilityManager.GetAbilityLevel(ConcernedAbility.Dash) == Upgrade.Upgrade1 || AbilityManager.GetAbilityLevel(ConcernedAbility.Dash) == Upgrade.Upgrade2)
		{
			maxStackAmount = upgradeMaxStackAmount;
		} else
		{
			maxStackAmount = defaultMaxStackAmount;
		}
		playerUI.GenerateDashBars();
	}
    #endregion

    #region Private functions
    private void UpdateStackAmount()
	{
		if (currentStackAmount < maxStackAmount)
		{
			float fillQuantity = (float)currentStackAmount / (float)maxStackAmount;
			float dashRecoverySpeedMultiplier = Mathf.Lerp(minStackRecoverySpeedMultiplier, maxStackRecoverySpeedMultiplier, dashRecoverySpeedCurve.Evaluate(fillQuantity));
			currentStackCooldown += Time.deltaTime * MomentumManager.GetValue(MomentumManager.datas.dashRecoverSpeedMultiplier) * dashRecoverySpeedMultiplier;
			if (currentStackCooldown >= defaultStackRecoveryDuration)
			{
				currentStackCooldown = 0;
				currentStackAmount += 1;
				FeedbackManager.SendFeedback("event.DashStackIncreased", linkedPawn);
			}
		}
	}
	private void ChangeState(DashState _newState)
	{
		switch (_newState)
		{
			case DashState.Dashing:
				linkedPawn.animator.SetBool("Dashing", true);
				break;
			case DashState.None:
				linkedPawn.animator.SetBool("Dashing", false);
				if (currentDashFX) { Destroy(currentDashFX); }
				break;
		}
		state = _newState;
	}
	private void GenerateClone()
	{
		GameObject i_clone = Instantiate(clonedVisuals.gameObject);
		Transform[] i_childTransforms = clonedVisuals.GetComponentsInChildren<Transform>();
		Transform[] i_cloneChildTransforms = i_clone.GetComponentsInChildren<Transform>();
		for (int i = 0; i < i_childTransforms.Length; i++)
		{
			i_cloneChildTransforms[i].position = i_childTransforms[i].position;
			i_cloneChildTransforms[i].localScale = i_childTransforms[i].localScale;
			i_cloneChildTransforms[i].rotation = i_childTransforms[i].rotation;
			var components = i_cloneChildTransforms[i].GetComponents(typeof(Component));
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
		AutoDestroyAfterDelay i_newComp = i_clone.AddComponent<AutoDestroyAfterDelay>();
		i_newComp.delay = cloneDuration;

	}
	public bool CanDash()
	{
		if (currentUseCooldown >= 0 || currentStackAmount <= 0)
		{
			return false;
		}
		return true;
	}
	private void UpdateCooldown()
	{
		if (currentUseCooldown >= 0)
		{
			currentUseCooldown -= Time.deltaTime;
		}
	}
	#endregion

	#region Coroutines
	IEnumerator Dash_C ( Vector3 _startPosition, Vector3 _endPosition )
	{
		currentUseCooldown = useCooldown;
		currentStackAmount--;
		currentDashFX = FeedbackManager.SendFeedback("event.Dash", this).GetVFX();

		Vector3 i_dashDirection = _endPosition - _startPosition;
		ChangeState(DashState.Dashing);
		float i_cloneCounter = 0;
		for (float i = 0; i <= duration; i += Time.deltaTime)
		{
			i_cloneCounter += Time.deltaTime;
			if (i_cloneCounter >= 1f/clonePerSec)
			{
				GenerateClone();
				i_cloneCounter = 0;
			}
			RaycastHit[] hits = Physics.SphereCastAll(linkedPawn.GetCenterPosition(), dashHitboxSize, i_dashDirection.normalized , 0.1f) ;
			foreach (RaycastHit hit in hits)
			{
				PlayerController hitPawn = hit.collider.transform.gameObject.GetComponent<PlayerController>();
				if (hitPawn && linkedPawn.isPlayer)
				{
					if (hitPawn != linkedPawn)
					{
						DashController dashController = hitPawn.GetComponent<DashController>();
						if (dashController != null)
						{
							dashController.StopAllCoroutines();
							dashController.ChangeState(DashState.None);
							linkedPawn.Push(PushType.Light, -i_dashDirection,PushForce.Force2);
						}
						hitPawn.Push(PushType.Light, i_dashDirection, PushForce.Force2);
						if (!unstoppableDash)
						{
							ChangeState(DashState.None);
							StopAllCoroutines();
						}
					}
				}
				if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Environment"))
				{
					if (linkedPawn.isPlayer)
						StartCoroutine(StopDash_C());
					//linkedPawn.WallSplat(WallSplatForce.Heavy, _startPosition - _endPosition);
				}
			}
			transform.position = Vector3.Lerp(_startPosition, _endPosition, dashSpeedCurve.Evaluate(i/duration));
			yield return null;
		}
		if (wallHit)
		{
			if (linkedPawn.isPlayer)
				StartCoroutine(StopDash_C());
		}
		transform.position = _endPosition;
		GenerateClone();
		ChangeState(DashState.None);
		StartCoroutine(FadePlayerSpeed());
		yield return null;
	}
	IEnumerator StopDash_C()
	{
		transform.position += Vector3.up * 2f;
		GenerateClone();
		ChangeState(DashState.None);
		linkedPawn.WallSplat(WallSplatForce.Light, -transform.forward, false);
		yield return null;
	}
	IEnumerator FadePlayerSpeed()
	{
		for (float i = 0; i < dashFadeDuration; i+=Time.deltaTime)
		{
			linkedPawn.AddSpeedModifier(new SpeedCoef(1 + dashFadeCurve.Evaluate(i / dashFadeDuration), Time.deltaTime, SpeedMultiplierReason.Dash, false));
			yield return null;
		}
	}
	#endregion

}
