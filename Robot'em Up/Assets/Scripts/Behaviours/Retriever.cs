using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class ReviveInformations
{
	public int amount;
	public int maxAmount;
	public PlayerController linkedPlayer;
	public AssemblingPartPanel linkedPanel;
}
public class Retriever : MonoBehaviour
{
    private Transform parent;
	private PassController passController;
	private PlayerController playerController;


	public List<ReviveInformations> retrievedParts = new List<ReviveInformations>();
	// Start is called before the first frame update
	void Awake()
    {
        parent = transform.parent;
		passController = GetComponentInParent<PassController>();
		playerController = GetComponentInParent<PlayerController>();
    }

	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "Ball")
		{
			FeedbackManager.SendFeedback("event.MagnetAttractingBall", this);
		}
	}
	private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ball")
        {
			BallBehaviour internal_ballBehaviour = other.GetComponent<BallBehaviour>();
			if (internal_ballBehaviour.GetState() == BallState.Grounded || internal_ballBehaviour.GetState() == BallState.Flying)
			{
				if (internal_ballBehaviour.GetState() == BallState.Flying)
				{
					if (internal_ballBehaviour.GetCurrentThrower() == playerController && (internal_ballBehaviour.GetCurrentBounceCount() < passController.minBouncesBeforePickingOwnBall || internal_ballBehaviour.GetTimeFlying() < passController.delayBeforePickingOwnBall)) { return; }
					passController.Receive(internal_ballBehaviour);
				}
				else
				{
					passController.Receive(internal_ballBehaviour);
				}
			}
        }
		if (other.tag == "CorePart")
		{
			CorePart internal_corePart = other.GetComponent<CorePart>();
			if (!internal_corePart.grounded) { return; }
			if (!internal_corePart.CanBePicked()) { return; }
			if (internal_corePart.linkedPawn != null)
			{
				SoundManager.PlaySound("PickUpOnePartOfItsBody", transform.position, transform);
				internal_corePart.Pick(playerController);
				bool internal_partsFound = false;
				List<ReviveInformations> newList = new List<ReviveInformations>();
				foreach (ReviveInformations parts in retrievedParts)
				{
					if (parts.linkedPlayer == internal_corePart.linkedPawn)
					{
						internal_partsFound = true;
						parts.amount++;
						parts.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = parts.amount + "/" + parts.maxAmount;
						parts.linkedPanel.GetComponent<Animator>().SetTrigger("showAmount");
						if (parts.amount >= parts.maxAmount)
						{
							parts.linkedPanel.GetComponent<Animator>().SetTrigger("showInstructions");
							playerController.AddRevivablePlayer(parts);
							FeedbackManager.SendFeedback("event.PickUpTheLastPartOfBody", this);
						}
						else
						{
							newList.Add(parts);
						}
					}
					else
					{
						newList.Add(parts);
					}
				}
				retrievedParts = newList;
				if (!internal_partsFound)
				{
					ReviveInformations internal_newPart = new ReviveInformations();
					internal_newPart.linkedPlayer = (PlayerController)internal_corePart.linkedPawn;
					internal_newPart.maxAmount = internal_corePart.totalPartCount;
					internal_newPart.amount = 1;
					internal_newPart.linkedPanel = Instantiate(Resources.Load<GameObject>("PlayerResource/CollectedPartsPanel"), GameManager.mainCanvas.transform).GetComponent<AssemblingPartPanel>();
					internal_newPart.linkedPanel.revivedPlayer = internal_newPart.linkedPlayer;
					internal_newPart.linkedPanel.revivingPlayer = playerController;
					internal_newPart.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = internal_newPart.amount + "/" + internal_corePart.totalPartCount;
					retrievedParts.Add(internal_newPart);
				}
			} else
			{
				//if (corePart.linkedPawn.GetType() == typeof(EnemyBehaviour)) (Must be added after heriting enemyBehaviour from pawnController)

				if (playerController.GetHealth() < playerController.GetMaxHealth())
				{
					internal_corePart.Pick(playerController);
									playerController.Heal(internal_corePart.healthValue);
				}
			}
		}
    }
}
