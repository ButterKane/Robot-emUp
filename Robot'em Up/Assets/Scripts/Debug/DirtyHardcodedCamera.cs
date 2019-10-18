using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtyHardcodedCamera : MonoBehaviour
{
	public PlayerController firstPlayer;
	public PlayerController secondPlayer;

	public void Update ()
	{
		Vector3 wantedPosition = new Vector3((firstPlayer.transform.position.x + secondPlayer.transform.position.x) / 2, 0, (firstPlayer.transform.position.z + secondPlayer.transform.position.z) / 2);
		transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime);
	}
}
