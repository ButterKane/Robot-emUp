using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSpiderAnimator : MonoBehaviour
{
	public List<ProceduralSpiderLegAnimator> forwardLegs;
	public List<ProceduralSpiderLegAnimator> backwardLegs;
	public float maxYOffset = 2f;
	public float maxRoll = 5f;
	public float moveSpeed = 4;
	private void LateUpdate ()
	{
		int groundedLegsAmount = 0;
		int rightGroundedLegs = 0;
		int leftGroundedLegs = 0;
		int forwardGroundedLegs = 0;
		int backwardGroundedLegs = 0;
		//Forward legs
		bool isMoving = false;
		foreach (ProceduralSpiderLegAnimator leg in forwardLegs)
		{
			leg.legSpeed = moveSpeed;
			if (!leg.isGrounded)
			{
				isMoving = true;
			} else
			{
				groundedLegsAmount++;
				forwardGroundedLegs++;
				if (leg.isRight)
				{
					rightGroundedLegs++;
				} else
				{
					leftGroundedLegs++;
				}
			}
		}
		if (!isMoving)
		{
			foreach (ProceduralSpiderLegAnimator leg in forwardLegs)
			{
				if (leg.ShouldMove())
				{
					leg.MoveLeg();
					break;
				}
			}
		}

		//Backward legs
		isMoving = false;
		foreach (ProceduralSpiderLegAnimator leg in backwardLegs)
		{
			leg.legSpeed = moveSpeed;
			if (!leg.isGrounded)
			{
				isMoving = true;
			}
			else
			{
				groundedLegsAmount++;
				backwardGroundedLegs++;
				if (leg.isRight)
				{
					rightGroundedLegs++;
				}
				else
				{
					leftGroundedLegs++;
				}
			}
		}
		if (!isMoving)
		{
			foreach (ProceduralSpiderLegAnimator leg in backwardLegs)
			{
				if (leg.ShouldMove())
				{
					leg.MoveLeg();
					break;
				}
			}
		}
		int totalLegAmount = backwardLegs.Count + forwardLegs.Count;
		float yOffset = Mathf.Lerp(0f, maxYOffset, 1 - ((float)groundedLegsAmount / (float)totalLegAmount));
		Vector3 newLocalPosition = transform.localPosition;
		newLocalPosition.y = yOffset;
		transform.localPosition = Vector3.Lerp(transform.localPosition, newLocalPosition, Time.deltaTime * (5f));

		float rightRoll = Mathf.Lerp(0f, 1f, (float)(rightGroundedLegs * 2f) / (float)totalLegAmount);
		float leftRoll = Mathf.Lerp(0f, -1f, (float)(leftGroundedLegs * 2f) / (float)totalLegAmount);
		float rollLerpValue = Mathf.Lerp(0f, 1f, (-(rightRoll + leftRoll)+ 1) / 2f);
		float roll = Mathf.Lerp(-maxRoll, maxRoll, 1- rollLerpValue);

		float forwardRoll = Mathf.Lerp(0f, 1f, (float)(forwardGroundedLegs * 2f) / (float)totalLegAmount);
		float backwardRoll = Mathf.Lerp(0f, -1f, (float)(backwardGroundedLegs * 2f) / (float)totalLegAmount);
		float yawLerpValue = Mathf.Lerp(0f, 1f, (-(forwardRoll + backwardRoll) + 1) / 2f);
		float yaw = Mathf.Lerp(-maxRoll, maxRoll, 1 - yawLerpValue);

		Vector3 newRotation = new Vector3(roll, transform.localEulerAngles.y, yaw);
		transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(newRotation), Time.deltaTime * (3.5f));
	}
}
