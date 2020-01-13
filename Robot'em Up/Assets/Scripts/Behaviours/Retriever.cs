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
			FeedbackManager.SendFeedback("event.PlayerAttractingBall", playerController, other.transform.position, other.transform.position - transform.position, other.transform.position - transform.position);
		}
	}
	private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ball")
        {
			BallBehaviour i_ballBehaviour = other.GetComponent<BallBehaviour>();
			if (i_ballBehaviour.GetState() == BallState.Grounded || i_ballBehaviour.GetState() == BallState.Flying)
			{
				if (i_ballBehaviour.GetState() == BallState.Flying)
				{
					if (i_ballBehaviour.GetCurrentThrower() == playerController && (i_ballBehaviour.GetCurrentBounceCount() < passController.minBouncesBeforePickingOwnBall || i_ballBehaviour.GetTimeFlying() < passController.delayBeforePickingOwnBall)) { return; }
					passController.Receive(i_ballBehaviour);
				}
				else
				{
					passController.Receive(i_ballBehaviour);
				}
			}
        }
		if (other.tag == "CorePart")
		{
			CorePart i_corePart = other.GetComponent<CorePart>();
			if (!i_corePart.grounded) { return; }
			if (!i_corePart.CanBePicked()) { return; }
			if (i_corePart.linkedPawn != null)
			{
				i_corePart.Pick(playerController);
				bool i_partsFound = false;
				List<ReviveInformations> newList = new List<ReviveInformations>();
				foreach (ReviveInformations parts in retrievedParts)
				{
					if (parts.linkedPlayer == i_corePart.linkedPawn)
					{
						i_partsFound = true;
						parts.amount++;
						parts.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = parts.amount + "/" + parts.maxAmount;
						parts.linkedPanel.GetComponent<Animator>().SetTrigger("showAmount");
						if (parts.amount >= parts.maxAmount)
						{
							FeedbackManager.SendFeedback("event.PlayerPickingLastBodyPart", playerController, other.transform.position, other.transform.position - transform.position, other.transform.position - transform.position);
							parts.linkedPanel.GetComponent<Animator>().SetTrigger("showInstructions");
							playerController.AddRevivablePlayer(parts);
						}
						else
						{
							FeedbackManager.SendFeedback("event.PlayerPickingBodyPart", playerController, other.transform.position, other.transform.position - transform.position, other.transform.position - transform.position);
							newList.Add(parts);
						}
					}
					else
					{
						newList.Add(parts);
					}
				}
				retrievedParts = newList;
				if (!i_partsFound)
				{
					ReviveInformations i_newPart = new ReviveInformations();
					i_newPart.linkedPlayer = (PlayerController)i_corePart.linkedPawn;
					i_newPart.maxAmount = i_corePart.totalPartCount;
					i_newPart.amount = 1;
					i_newPart.linkedPanel = Instantiate(Resources.Load<GameObject>("PlayerResource/CollectedPartsPanel"), GameManager.mainCanvas.transform).GetComponent<AssemblingPartPanel>();
					i_newPart.linkedPanel.revivedPlayer = i_newPart.linkedPlayer;
					i_newPart.linkedPanel.revivingPlayer = playerController;
					i_newPart.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = i_newPart.amount + "/" + i_corePart.totalPartCount;
					retrievedParts.Add(i_newPart);
				}
			} else
			{
				//if (corePart.linkedPawn.GetType() == typeof(EnemyBehaviour)) (Must be added after heriting enemyBehaviour from pawnController)

				if (playerController.GetHealth() < playerController.GetMaxHealth())
				{
					i_corePart.Pick(playerController);
									playerController.Heal(i_corePart.healthValue);
				}
			}
		}
    }
}
