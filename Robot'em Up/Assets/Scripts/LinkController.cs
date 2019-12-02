using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
	[Header("Settings")]
	public PawnController firstPawn;
	public PawnController secondPawn;

	public float maxDistanceBeforeShowing = 10f;
	public float maxDistanceBeforeSlowing = 12f;
	public float maxDistanceBeforeBreaking = 14f;
	public float distanceBeforeRebuilding = 10f;

	public Gradient linkColor;

	public float maxSlowCoef = 0.2f;

	public float damagesPerSecWithoutLink = 1f;

	public AnimationCurve slowCoefCurve;
	public AnimationCurve opacityFadeCurve;

	public Material linkMaterial;
	public float linkWidth;

	private GameObject linkGameObject;
	private LineRenderer lineRenderer;

	private bool linkIsBroke;
	private float damageCount;

	private LinkState linkState;

	private void Awake ()
	{
		linkGameObject = GenerateLinkHolder();
		ChangeLinkState(LinkState.Hidden);
	}

	GameObject GenerateLinkHolder()
	{
		GameObject newLinkHolder = new GameObject();
		newLinkHolder.name = "Link[" + firstPawn.name + "] - [" + secondPawn.name + "]";
		newLinkHolder.transform.SetParent(firstPawn.transform.parent);
		lineRenderer = newLinkHolder.AddComponent<LineRenderer>();
		lineRenderer.material = linkMaterial;
		lineRenderer.startWidth = linkWidth;
		lineRenderer.endWidth = linkWidth;
		return newLinkHolder;
	}

	private void Update ()
	{
		UpdateLink();
	}

	private void ChangeLinkState ( LinkState _newState )
	{
		if (linkState == _newState) { return; }

		switch (_newState)
		{
			case LinkState.Broken:
				WarningPanel.OpenPanel();
				break;
			case LinkState.Rebuilt:
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
		if (linkGameObject != null)
		{
			float linkLength = Vector3.Distance(firstPawn.transform.position, secondPawn.transform.position);
			if (!linkIsBroke)
			{
				if (linkLength < maxDistanceBeforeBreaking)
				{
					//Hide link
					lineRenderer.positionCount = 0;
					ChangeLinkState(LinkState.Hidden);
				}
				if (linkLength >= maxDistanceBeforeShowing && linkLength < maxDistanceBeforeSlowing)
				{
					//Show link
					lineRenderer.positionCount = 2;
					lineRenderer.SetPosition(0, firstPawn.GetCenterPosition());
					lineRenderer.SetPosition(1, secondPawn.GetCenterPosition());
					float lerpValue = (maxDistanceBeforeSlowing - linkLength) / (maxDistanceBeforeSlowing - maxDistanceBeforeShowing);
					lerpValue = 1f-slowCoefCurve.Evaluate(lerpValue);
					Color transparentColor = linkColor.Evaluate(0);
					transparentColor.a = 0;
					lineRenderer.startColor = Color.Lerp(transparentColor, linkColor.Evaluate(0), lerpValue);
					lineRenderer.endColor = Color.Lerp(transparentColor, linkColor.Evaluate(0), lerpValue);
					ChangeLinkState(LinkState.Showing);
				}
				if (linkLength >= maxDistanceBeforeSlowing && linkLength < maxDistanceBeforeBreaking)
				{
					lineRenderer.positionCount = 2;
					lineRenderer.SetPosition(0, firstPawn.GetCenterPosition());
					lineRenderer.SetPosition(1, secondPawn.GetCenterPosition());
					float lerpValue = (maxDistanceBeforeBreaking - linkLength) / (maxDistanceBeforeBreaking - maxDistanceBeforeSlowing);
					lerpValue = 1f-slowCoefCurve.Evaluate(lerpValue);
					lineRenderer.startColor = linkColor.Evaluate(lerpValue);
					lineRenderer.endColor = linkColor.Evaluate(lerpValue);

					float slowValue = Mathf.Lerp(1f, maxSlowCoef, lerpValue);

					//Slow player 1
					float FcDirectionAngle = Vector3.Angle(firstPawn.transform.forward, secondPawn.transform.position - firstPawn.transform.position);
					float FcSlowValue = Mathf.Lerp(1f, slowValue, FcDirectionAngle / 180f);
					firstPawn.AddSpeedCoef(new SpeedCoef(FcSlowValue, Time.deltaTime, SpeedModifierReason.Link, false));

					//Slow player 2
					float FsDirectionAngle = Vector3.Angle(secondPawn.transform.forward, firstPawn.transform.position - secondPawn.transform.position);
					float FsSlowValue = Mathf.Lerp(1f, slowValue, FsDirectionAngle / 180f);
					secondPawn.AddSpeedCoef(new SpeedCoef(FsSlowValue, Time.deltaTime, SpeedModifierReason.Link, false));
					ChangeLinkState(LinkState.Slowing);
				}
				if (linkLength >= maxDistanceBeforeBreaking)
				{
					//Break link
					lineRenderer.positionCount = 0;
					linkIsBroke = true;
					ChangeLinkState(LinkState.Broken);
				}
			}
			else
			{
				if (linkLength <= distanceBeforeRebuilding)
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
}
