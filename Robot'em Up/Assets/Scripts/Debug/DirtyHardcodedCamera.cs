using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtyHardcodedCamera : MonoBehaviour
{
	public PawnController firstPawn;
	public PawnController secondPawn;

	public void Update ()
	{
		Vector3 internal_wantedPosition = new Vector3((firstPawn.transform.position.x + secondPawn.transform.position.x) / 2, (firstPawn.transform.position.y + secondPawn.transform.position.y) / 2, (firstPawn.transform.position.z + secondPawn.transform.position.z) / 2);
		transform.position = Vector3.Lerp(transform.position, internal_wantedPosition, Time.deltaTime);
	}
}
