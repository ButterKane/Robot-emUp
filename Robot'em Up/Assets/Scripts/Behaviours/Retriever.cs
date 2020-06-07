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
	private PassController passController;
	private PlayerController playerController;
	private PawnController pawnController;

	public List<ReviveInformations> retrievedParts = new List<ReviveInformations>();
	// Start is called before the first frame update
	private void Start()
    {
		playerController = GetComponentInParent<PlayerController>();
		pawnController = GetComponentInParent<PawnController>();
		passController = pawnController.passController;
    }
	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "Ball")
		{
			BallBehaviour i_ballBehaviour = other.GetComponent<BallBehaviour>();
			if (i_ballBehaviour.GetState() == BallState.Flying)
			{
				if (i_ballBehaviour.GetCurrentThrower() == pawnController && (i_ballBehaviour.GetCurrentBounceCount() < passController.minBouncesBeforePickingOwnBall || i_ballBehaviour.GetTimeFlying() < passController.delayBeforePickingOwnBall)) { return; }
				passController.Receive(i_ballBehaviour);
			}
			else if (i_ballBehaviour.GetState() == BallState.Grounded)
			{
				passController.Receive(i_ballBehaviour);
			}
		}
		if (other.tag == "GrabbableTrigger" && playerController)
		{
			GrabbableInformation grabbableInformation = other.GetComponent<GrabbableInformation>();
			grabbableInformation.EnablePreviewForPlayer(playerController);
			playerController.targetedGrabbable.Add(grabbableInformation);
		}
	}
	private void OnTriggerExit ( Collider other )
	{
		if (other.tag == "GrabbableTrigger" && playerController)
		{
			GrabbableInformation grabbableInformation = other.GetComponent<GrabbableInformation>();
			grabbableInformation.DisablePreviewForPlayer(playerController);
			playerController.targetedGrabbable.Remove(grabbableInformation);
		}
	}
	private void OnTriggerStay(Collider other)
    {
		if (other.tag == "CorePart" && playerController)
		{
			CorePart i_corePart = other.GetComponent<CorePart>();
			if (!i_corePart.grounded) { return; }
			if (!i_corePart.CanBePicked()) { return; }
			if (i_corePart.linkedPawn != null)
			{
				RetrieveCorePart(i_corePart);
			} else
			{
				i_corePart.Pick(playerController);
				playerController.Heal(i_corePart.healthValue);
			}
		}
    }

	#region Public functions
	public void AllowPlayerRevive(ReviveInformations parts)
	{
		FeedbackManager.SendFeedback("event.PlayerPickingLastBodyPart", playerController, playerController.transform.position, playerController.transform.position - transform.position, playerController.transform.position - transform.position);
		parts.linkedPanel.GetComponent<Animator>().SetTrigger("showInstructions");
		playerController.AddRevivablePlayer(parts);
	}
	public void AllowPlayerRevive(PlayerController player)
	{
		ReviveInformations i_newPart = new ReviveInformations();
		i_newPart.linkedPlayer = player;
		i_newPart.maxAmount = player.revivePartsCount;
		i_newPart.amount = player.revivePartsCount;
		i_newPart.linkedPanel = Instantiate(Resources.Load<GameObject>("PlayerResource/CollectedPartsPanel"), GameManager.mainCanvas.transform).GetComponent<AssemblingPartPanel>();
		i_newPart.linkedPanel.revivedPlayer = i_newPart.linkedPlayer;
		i_newPart.linkedPanel.revivingPlayer = playerController;
		i_newPart.linkedPanel.Init();
		i_newPart.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = i_newPart.amount + "/" + player.revivePartsCount;
		AllowPlayerRevive(i_newPart);
	}
	public void RetrieveCorePart(CorePart i_corePart)
	{
		i_corePart.Pick(playerController);
        FeedbackManager.SendFeedback("event.PlayerPickingBodyPart", playerController, i_corePart.transform.position, i_corePart.transform.position - transform.position, i_corePart.transform.position - transform.position);
        bool i_partsFound = false;
		List<ReviveInformations> newList = new List<ReviveInformations>();
		foreach (ReviveInformations i_parts in retrievedParts)
		{
			if (i_parts.linkedPlayer == i_corePart.linkedPawn)
			{
				i_partsFound = true;
				i_parts.amount++;
				if (i_parts.amount >= i_parts.maxAmount)
				{
					AllowPlayerRevive(i_parts);
				}
				else
				{
					i_parts.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = i_parts.amount + "/" + i_parts.maxAmount;
					i_parts.linkedPanel.GetComponent<Animator>().SetTrigger("showAmount");
					newList.Add(i_parts);
				}
			}
			else
			{
				newList.Add(i_parts);
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
			i_newPart.linkedPanel.Init();
			i_newPart.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = i_newPart.amount + "/" + i_corePart.totalPartCount;
			retrievedParts.Add(i_newPart);
		}
	}
	#endregion
}
