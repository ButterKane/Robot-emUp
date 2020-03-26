using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum TriggerCondition { OnePlayer, TwoPlayer, Middlepoint }
[RequireComponent(typeof(Collider))]
public class CustomTrigger : MonoBehaviour
{
	public bool onExit;
	public bool singleUse;
	public bool useDirection = true;
	public TriggerCondition triggerCondition;
	public UnityEvent onTriggerEnterAction;
	public UnityEvent onTriggerComebackAction;

    private List<PlayerController> playerGoneThroughDoor;
	private Collider col;
	private bool used;
	private int direction;

	private void Awake ()
	{
		col = GetComponent<Collider>();
		playerGoneThroughDoor = new List<PlayerController>();
		direction = 1;
    }

	void CheckTrigger( Collider other )
	{
		if (other.tag == "MiddlePoint" && triggerCondition == TriggerCondition.Middlepoint)
		{
			TriggerEvent();
			return;
		}
		if (other.gameObject.tag == "Player")
		{
			PlayerController potentialPlayer = other.transform.gameObject.GetComponent<PlayerController>();
			if (potentialPlayer != null)
			{
				if (useDirection == false || transform.InverseTransformPoint(potentialPlayer.transform.position).z * direction > 0.0)
				{
					if (!playerGoneThroughDoor.Contains(potentialPlayer))
					{
						playerGoneThroughDoor.Add(potentialPlayer);
						if (triggerCondition == TriggerCondition.OnePlayer)
						{
							TriggerEvent();
						}
						if (triggerCondition == TriggerCondition.TwoPlayer && playerGoneThroughDoor.Count > 1)
						{
							TriggerEvent();
						}
					}
				}
				else
				{
					if (playerGoneThroughDoor.Contains(potentialPlayer))
					{
						playerGoneThroughDoor.Remove(potentialPlayer);
					}
				}
			}
		}
	}
	private void OnTriggerEnter ( Collider other )
	{
		if (!onExit)
		{
			CheckTrigger(other);
		}
	}

	private void OnTriggerExit ( Collider other )
	{
		if (onExit)
		{
			CheckTrigger(other);
		}
	}

	void TriggerEvent()
	{
		playerGoneThroughDoor.Clear();
		if (singleUse)
		{
			if (used)
			{
				return;
			} else
			{
				used = true;
				onTriggerEnterAction.Invoke();
			}
		} else
		{
			if (direction == 1)
			{
				onTriggerEnterAction.Invoke();
			} else
			{
				onTriggerComebackAction.Invoke();
			}
			direction = -direction;
		}
	}

}
