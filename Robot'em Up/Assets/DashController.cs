using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DashState
{
	Dashing,
	None
}
public class DashController : MonoBehaviour
{
	[Header("Settings")]
	public float minDistance = 2f;
	public float maxDistance = 3f;
	public float speed = 10f;

	public bool unstoppableDash;
	public bool invincibleDuringDash;

	public float cooldown = 3f;

	public DashState state;

	public Transform visuals;

	private PlayerController linkedPlayer;
	private float currentCooldown;
	private GameObject currentDashFX;

	public float clonePerSec = 10f;
	public float cloneDuration = 0.5f;

	public Material cloneMaterial;

	public GameObject dashTrail;
	public GameObject startDashFX;
	public GameObject endDashFX;
	private void Awake ()
	{
		linkedPlayer = GetComponent<PlayerController>();
	}

	private void Update ()
	{
		UpdateCooldown();
	}
	public void Dash()
	{
		Vector3 startPosition = transform.position;
		Vector3 endPosition = transform.position + transform.forward * maxDistance;
		//Check for min distance & maxDistance
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, LayerMask.GetMask("Environment")))
		{
			if (Vector3.Distance(transform.position, hit.point) <= minDistance)
			{
				return; //Cancel dash
			} else
			{
				endPosition = hit.point - (transform.forward * 0.2f);
			}
		}
		endPosition.y = startPosition.y;

		StartCoroutine(Dash_C(startPosition, endPosition));
		currentCooldown = cooldown;
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
		GameObject clone = Instantiate(visuals.gameObject);
		Transform[] childTransforms = visuals.GetComponentsInChildren<Transform>();
		Transform[] cloneChildTransforms = clone.GetComponentsInChildren<Transform>();
		for (int i = 0; i < childTransforms.Length; i++)
		{
			cloneChildTransforms[i].position = childTransforms[i].position;
			cloneChildTransforms[i].localScale = childTransforms[i].localScale;
			cloneChildTransforms[i].rotation = childTransforms[i].rotation;
			var components = cloneChildTransforms[i].GetComponents(typeof(Component));
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
		AutoDestroyAfterDelay newComp = clone.AddComponent<AutoDestroyAfterDelay>();
		newComp.delay = cloneDuration;

	}

	public bool CanDash()
	{
		if (currentCooldown >= 0)
		{
			return false;
		}
		return true;
	}

	void UpdateCooldown()
	{
		if (currentCooldown >= 0)
		{
			currentCooldown -= Time.deltaTime;
		}
	}

	IEnumerator Dash_C ( Vector3 _startPosition, Vector3 _endPosition )
	{
		ChangeState(DashState.Dashing);
		if (invincibleDuringDash) { linkedPlayer.SetInvincible(true); }
		float cloneCounter = 0;
		for (float i = 0; i < Vector3.Distance(_startPosition, _endPosition); i += Time.deltaTime * speed)
		{
			cloneCounter += Time.deltaTime;
			if (cloneCounter >= 1f/clonePerSec)
			{
				GenerateClone();
				cloneCounter = 0;
			}
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, 0.1f, LayerMask.GetMask("Environment")))
			{
				StopAllCoroutines();
			}
			transform.position = Vector3.Lerp(_startPosition, _endPosition, i / Vector3.Distance(_startPosition, _endPosition));
			yield return new WaitForEndOfFrame();
		}
		transform.position = _endPosition;
		ChangeState(DashState.None);
		linkedPlayer.SetInvincible(false);
	}

}
