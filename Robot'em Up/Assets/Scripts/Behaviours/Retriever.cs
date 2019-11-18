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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ball")
        {
			BallBehaviour ballBehaviour = other.GetComponent<BallBehaviour>();
			if (ballBehaviour.GetState() == BallState.Grounded || ballBehaviour.GetState() == BallState.Flying)
			{
				if (ballBehaviour.GetState() == BallState.Flying)
				{
					if (!playerController.enablePickOwnBall && ballBehaviour.GetCurrentThrower() == playerController)
					{
						return;
					} else
					{
						passController.Receive(ballBehaviour);
					}
				}
				else
				{
					passController.Receive(ballBehaviour);
				}
			}
        }
		if (other.tag == "PlayerPart")
		{
			PlayerPart playerPart = other.GetComponent<PlayerPart>();
			if (!playerPart.grounded) { return; }
			playerPart.Pick(playerController);
			bool partsFound = false;
			List<ReviveInformations> newList = new List<ReviveInformations>();
			foreach (ReviveInformations parts in retrievedParts)
			{
				if (parts.linkedPlayer == playerPart.linkedPlayer) {
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
				newPart.linkedPlayer = playerPart.linkedPlayer;
				newPart.maxAmount = playerPart.totalPartCount;
				newPart.amount = 1;
				newPart.linkedPanel = Instantiate(Resources.Load<GameObject>("PlayerResource/CollectedPartsPanel"), FindObjectOfType<Canvas>().transform).GetComponent<AssemblingPartPanel>();
				newPart.linkedPanel.revivedPlayer = newPart.linkedPlayer;
				newPart.linkedPanel.revivingPlayer = playerController;
				newPart.linkedPanel.transform.Find("TextHolder").transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = newPart.amount + "/" + playerPart.totalPartCount;
				retrievedParts.Add(newPart);
			}
		}
    }
}
