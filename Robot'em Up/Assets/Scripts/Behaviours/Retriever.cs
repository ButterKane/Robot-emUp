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

	private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ball")
        {
			BallBehaviour ballBehaviour = other.GetComponent<BallBehaviour>();
			if (ballBehaviour.GetState() == BallState.Grounded || ballBehaviour.GetState() == BallState.Flying)
			{
				if (ballBehaviour.GetState() == BallState.Flying)
				{
					if (ballBehaviour.GetCurrentThrower() == playerController && (ballBehaviour.GetCurrentBounceCount() < passController.minBouncesBeforePickingOwnBall || ballBehaviour.GetTimeFlying() < passController.delayBeforePickingOwnBall)) { return; }
					passController.Receive(ballBehaviour);
				}
				else
				{
					passController.Receive(ballBehaviour);
				}
			}
        }
		if (other.tag == "CorePart")
		{
			CorePart corePart = other.GetComponent<CorePart>();
			if (!corePart.grounded) { return; }
			if (!corePart.CanBePicked()) { return; }
			if (corePart.linkedPawn != null)
			{
				SoundManager.PlaySound("PickUpOnePartOfItsBody", transform.position, transform);
				corePart.Pick(playerController);
				bool partsFound = false;
				List<ReviveInformations> newList = new List<ReviveInformations>();
				foreach (ReviveInformations parts in retrievedParts)
				{
					if (parts.linkedPlayer == corePart.linkedPawn)
					{
						partsFound = true;
						parts.amount++;
						parts.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = parts.amount + "/" + parts.maxAmount;
						parts.linkedPanel.GetComponent<Animator>().SetTrigger("showAmount");
						if (parts.amount >= parts.maxAmount)
						{
							parts.linkedPanel.GetComponent<Animator>().SetTrigger("showInstructions");
							playerController.AddRevivablePlayer(parts);
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
				if (!partsFound)
				{
					ReviveInformations newPart = new ReviveInformations();
					newPart.linkedPlayer = (PlayerController)corePart.linkedPawn;
					newPart.maxAmount = corePart.totalPartCount;
					newPart.amount = 1;
					newPart.linkedPanel = Instantiate(Resources.Load<GameObject>("PlayerResource/CollectedPartsPanel"), GameManager.mainCanvas.transform).GetComponent<AssemblingPartPanel>();
					newPart.linkedPanel.revivedPlayer = newPart.linkedPlayer;
					newPart.linkedPanel.revivingPlayer = playerController;
					newPart.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = newPart.amount + "/" + corePart.totalPartCount;
					retrievedParts.Add(newPart);
				}
			} else
			{
				//if (corePart.linkedPawn.GetType() == typeof(EnemyBehaviour)) (Must be added after heriting enemyBehaviour from pawnController)

				if (playerController.GetHealth() < playerController.GetMaxHealth())
				{
					corePart.Pick(playerController);
									playerController.Heal(corePart.healthValue);
				}
			}
		}
    }
}
