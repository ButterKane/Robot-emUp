using System.Collections;
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

	private PawnController linkedPawn;
	private float currentUseCooldown;
	private float currentStackCooldown;
	private int currentStackAmount;

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
		Vector3 i_startPosition = transform.position;
		Vector3 i_endPosition = transform.position + transform.forward * maxDistance; 
		//Check for min distance & maxDistance
		RaycastHit hit;
		if (Physics.Raycast(linkedPawn.GetCenterPosition(), transform.forward, out hit, maxDistance))
		{
			if (Vector3.Distance(linkedPawn.GetCenterPosition(), hit.point) <= minDistance)
			{
				return; //Cancel dash
			} else
			{
				i_endPosition = hit.point - (transform.forward * 0.5f);
			}
		}
		i_endPosition.y = i_startPosition.y;

		currentStackAmount--;
		StartCoroutine(Dash_C(i_startPosition, i_endPosition));
		currentUseCooldown = useCooldown;
	}
	void ChangeState(DashState _newState)
	{
		switch (_newState)
		{
			case DashState.Dashing:
				break;
			case DashState.None:
				break;
		}
		state = _newState;
	}

	void GenerateClone()
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
		float i_cloneCounter = 0;
		for (float i = 0; i < Vector3.Distance(_startPosition, _endPosition); i += Time.deltaTime * speed)
		{
			i_cloneCounter += Time.deltaTime;
			if (i_cloneCounter >= 1f/clonePerSec)
			{
				GenerateClone();
				i_cloneCounter = 0;
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
