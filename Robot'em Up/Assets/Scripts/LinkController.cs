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

[RequireComponent(typeof(PlayerController))]
public class LinkController : MonoBehaviour
{
	[Header("Settings")]
	public PlayerController firstController;
	public PlayerController secondController;

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
		newLinkHolder.name = "Link[" + firstController.name + "] - [" + secondController.name + "]";
		newLinkHolder.transform.SetParent(firstController.transform.parent);
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
			float linkLength = Vector3.Distance(firstController.transform.position, secondController.transform.position);
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
					lineRenderer.SetPosition(0, firstController.GetCenterPosition());
					lineRenderer.SetPosition(1, secondController.GetCenterPosition());
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
					lineRenderer.SetPosition(0, firstController.GetCenterPosition());
					lineRenderer.SetPosition(1, secondController.GetCenterPosition());
					float lerpValue = (maxDistanceBeforeBreaking - linkLength) / (maxDistanceBeforeBreaking - maxDistanceBeforeSlowing);
					lerpValue = 1f-slowCoefCurve.Evaluate(lerpValue);
					lineRenderer.startColor = linkColor.Evaluate(lerpValue);
					lineRenderer.endColor = linkColor.Evaluate(lerpValue);

					float slowValue = Mathf.Lerp(1f, maxSlowCoef, lerpValue);

					//Slow player 1
					float FcDirectionAngle = Vector3.Angle(firstController.transform.forward, secondController.transform.position - firstController.transform.position);
					float FcSlowValue = Mathf.Lerp(1f, slowValue, FcDirectionAngle / 180f);
					firstController.AddSpeedCoef(new SpeedCoef(FcSlowValue, Time.deltaTime, SlowReason.Link, false));

					//Slow player 2
					float FsDirectionAngle = Vector3.Angle(secondController.transform.forward, firstController.transform.position - secondController.transform.position);
					float FsSlowValue = Mathf.Lerp(1f, slowValue, FsDirectionAngle / 180f);
					secondController.AddSpeedCoef(new SpeedCoef(FsSlowValue, Time.deltaTime, SlowReason.Link, false));
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
						firstController.DamagePlayer(Mathf.RoundToInt(damageCount));
						secondController.DamagePlayer(Mathf.RoundToInt(damageCount));
						damageCount = 0;
					}
				}
			}
		}
	}
}
