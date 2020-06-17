using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public enum LinkState
{
	Broken,
	Showing,
	Slowing,
	Rebuilt,
	Hidden
}

[RequireComponent(typeof(PawnController))]
public class LinkController : MonoBehaviour
{
	[Separator("General Settings")]
	public static LinkController i;
	[ReadOnly] public PawnController firstPawn;
	[ReadOnly] public PawnController secondPawn;

	public float maxDistanceBeforeShowing = 10f;
	public float maxDistanceBeforeSlowing = 12f;
	public float maxDistanceBeforeBreaking = 14f;
	public float distanceBeforeRebuilding = 10f;
	public float maxSlowCoef = 0.2f;
	public float damagesPerSecWithoutLink = 1f;

	public AnimationCurve slowCoefCurve;

	[Separator("Visuals")]
	public Material linkMaterial;
	public float linkWidth;
	public AnimationCurve opacityFadeCurve;

	private GameObject linkGameObject;
	private LineRenderer lineRenderer;

	private bool linkIsBroke;
	private float damageCount;

	private LinkState linkState;
	private bool linkDisabled;

	private void Start ()
	{
        if (firstPawn == null)
        {
            firstPawn = GameManager.playerOne;
        }

        if (secondPawn == null)
        {
            secondPawn = GameManager.playerTwo;
        }
        linkGameObject = GenerateLinkHolder();
		i = this;
		ChangeLinkState(LinkState.Hidden);
    }
	private void Update ()
	{
		UpdateLink();
	}

	#region Private functions
	private GameObject GenerateLinkHolder ()
	{
		GameObject i_newLinkHolder = new GameObject();
		i_newLinkHolder.name = "Link[" + firstPawn.name + "] - [" + secondPawn.name + "]";
		i_newLinkHolder.transform.SetParent(firstPawn.transform.parent);
		lineRenderer = i_newLinkHolder.AddComponent<LineRenderer>();
		lineRenderer.material = linkMaterial;
		lineRenderer.startWidth = linkWidth;
		lineRenderer.endWidth = linkWidth;
		DontDestroyOnLoad(i_newLinkHolder.gameObject);
		GameManager.DDOL.Add(i_newLinkHolder.gameObject);
		return i_newLinkHolder;
	}

	public void DisableLink()
	{
		lineRenderer.positionCount = 0;
		linkDisabled = true;
	}

	public void EnableLink()
	{
		linkDisabled = false;
	}
	private void ChangeLinkState ( LinkState _newState )
	{
		if (linkState == _newState) { return; }

		switch (_newState)
		{
			case LinkState.Broken:
				FeedbackManager.SendFeedback("event.LinkBroken", firstPawn, firstPawn.GetCenterPosition(), firstPawn.GetCenterPosition() - transform.position, firstPawn.GetCenterPosition() - transform.position);
				FeedbackManager.SendFeedback("event.LinkBroken", secondPawn, secondPawn.GetCenterPosition(), secondPawn.GetCenterPosition() - transform.position, secondPawn.GetCenterPosition() - transform.position);
				WarningPanel.OpenPanel();
				break;
			case LinkState.Rebuilt:
				FeedbackManager.SendFeedback("event.LinkRebuilt", firstPawn, firstPawn.GetCenterPosition(), firstPawn.GetCenterPosition() - transform.position, firstPawn.GetCenterPosition() - transform.position);
				FeedbackManager.SendFeedback("event.LinkRebuilt", secondPawn, secondPawn.GetCenterPosition(), secondPawn.GetCenterPosition() - transform.position, secondPawn.GetCenterPosition() - transform.position);
				WarningPanel.ClosePanel();
				break;
			case LinkState.Showing:
				break;
			case LinkState.Slowing:
				break;
			case LinkState.Hidden:
				break;
		}

		linkState = _newState;
	}
	private void UpdateLink()
	{
		if (linkGameObject != null || linkDisabled)
		{
			if (firstPawn.moveState == MoveState.Dead || secondPawn.moveState == MoveState.Dead) { lineRenderer.positionCount = 0; WarningPanel.ClosePanelInstantly(); linkIsBroke = false; return;}
			float i_linkLength = Vector3.Distance(firstPawn.transform.position, secondPawn.transform.position);
			if (!linkIsBroke)
			{
				LockEnemiesInPath();
				linkMaterial.SetFloat("_CurrentEnergyAmout", EnergyManager.GetEnergy());
				if (i_linkLength < maxDistanceBeforeBreaking)
				{
					//Hide link
					lineRenderer.positionCount = 2;
					lineRenderer.SetPosition(0, firstPawn.GetCenterPosition());
					lineRenderer.SetPosition(1, secondPawn.GetCenterPosition());
					linkMaterial.SetFloat("_BreakingLinkProgression", 0);
					ChangeLinkState(LinkState.Hidden);
				}
				if (i_linkLength >= maxDistanceBeforeShowing && i_linkLength < maxDistanceBeforeSlowing)
				{
					//Show link
					lineRenderer.positionCount = 2;
					lineRenderer.SetPosition(0, firstPawn.GetCenterPosition());
					lineRenderer.SetPosition(1, secondPawn.GetCenterPosition());
					float lerpValue = (maxDistanceBeforeSlowing - i_linkLength) / (maxDistanceBeforeSlowing - maxDistanceBeforeShowing);
					lerpValue = 1f-slowCoefCurve.Evaluate(lerpValue);
					linkMaterial.SetFloat("_BreakingLinkProgression", 0);
					ChangeLinkState(LinkState.Showing);
				}
				if (i_linkLength >= maxDistanceBeforeSlowing && i_linkLength < maxDistanceBeforeBreaking)
				{
					lineRenderer.positionCount = 2;
					lineRenderer.SetPosition(0, firstPawn.GetCenterPosition());
					lineRenderer.SetPosition(1, secondPawn.GetCenterPosition());
					float lerpValue = (maxDistanceBeforeBreaking - i_linkLength) / (maxDistanceBeforeBreaking - maxDistanceBeforeSlowing);
					lerpValue = 1f-slowCoefCurve.Evaluate(lerpValue);
					linkMaterial.SetFloat("_BreakingLinkProgression", lerpValue);

					float slowValue = Mathf.Lerp(1f, maxSlowCoef, lerpValue);

					//Slow player 1
					float FcDirectionAngle = Vector3.Angle(firstPawn.transform.forward, secondPawn.transform.position - firstPawn.transform.position);
					float FcSlowValue = Mathf.Lerp(1f, slowValue, FcDirectionAngle / 180f);
					firstPawn.AddSpeedModifier(new SpeedCoef(FcSlowValue, Time.deltaTime, SpeedMultiplierReason.Link, false));

					//Slow player 2
					float FsDirectionAngle = Vector3.Angle(secondPawn.transform.forward, firstPawn.transform.position - secondPawn.transform.position);
					float FsSlowValue = Mathf.Lerp(1f, slowValue, FsDirectionAngle / 180f);
					secondPawn.AddSpeedModifier(new SpeedCoef(FsSlowValue, Time.deltaTime, SpeedMultiplierReason.Link, false));
					ChangeLinkState(LinkState.Slowing);
				}
				if (i_linkLength >= maxDistanceBeforeBreaking)
				{
					//Break link
					lineRenderer.positionCount = 0;
					linkIsBroke = true;
					ChangeLinkState(LinkState.Broken);
				}
			}
			else
			{
				if (i_linkLength <= distanceBeforeRebuilding)
				{
					//Rebuild link
					linkIsBroke = false;
					ChangeLinkState(LinkState.Rebuilt);
				} else
				{
					damageCount += damagesPerSecWithoutLink * Time.deltaTime;
					if (damageCount >= 1)
					{
						firstPawn.Damage(Mathf.RoundToInt(damageCount));
						secondPawn.Damage(Mathf.RoundToInt(damageCount));
						damageCount = 0;
					}
				}
			}
		}
	}

	private void LockEnemiesInPath ()
	{
		List<Vector3> pathCoordinates = new List<Vector3>();
		pathCoordinates.Add(firstPawn.GetCenterPosition());
		pathCoordinates.Add(secondPawn.GetCenterPosition());
		LockManager.LockTargetsInPath(pathCoordinates, 0, true, false);
	}
	#endregion
}
