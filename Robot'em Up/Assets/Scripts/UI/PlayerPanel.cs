using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using XInputDotNetPure;

public class PlayerPanel : MonoBehaviour
{
	public PlayerIndex playerIndex;
	[HideInInspector] public PlayerController linkedPlayer;

	private void Awake ()
	{
		foreach (PlayerController player in FindObjectsOfType<PlayerController>())
		{
			if (player.playerIndex == playerIndex)
			{
				linkedPlayer = player;
			}
		}
	}
}
