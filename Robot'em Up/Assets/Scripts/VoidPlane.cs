﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidPlane : MonoBehaviour
{
	private void OnTriggerEnter ( Collider other )
	{
		if (other.tag == "CorePart")
		{
			CorePart i_corePart = other.GetComponent<CorePart>();
			//Find retriever of the alive player
			if (GameManager.alivePlayers.Count <= 1)
			{
				Retriever r = GameManager.alivePlayers[0].GetComponentInChildren<Retriever>();
				r.RetrieveCorePart(i_corePart);
			}
		}

		if (other.tag == "Player")
		{
			PlayerController player = other.GetComponent<PlayerController>();
			if (player == null) { return; }
			player.KillWithoutCorePart();
			if (GameManager.alivePlayers.Count <= 1 && GameManager.alivePlayers.Count > 0)
			{
				Retriever r = GameManager.alivePlayers[0].GetComponentInChildren<Retriever>();
				r.AllowPlayerRevive(player);
			}
		}
	}
}
