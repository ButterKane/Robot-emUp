using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
	public List<GrabbableInformation> grabbableInformation = new List<GrabbableInformation>();

	public void UpdateLine()
	{
		foreach (GrabbableInformation gi in grabbableInformation)
		{
			if (gi.trigger != null && gi.previewLine != null)
			{
				gi.previewLine.positionCount = 2;
				gi.previewLine.SetPosition(0, gi.trigger.center + gi.trigger.transform.position);
				gi.previewLine.SetPosition(1, gi.targetedPosition.position);
			}
		}
	}
}

public class PlayerGrabPreview : MonoBehaviour
{
	public PlayerController player;
	public LineRenderer lr;
}