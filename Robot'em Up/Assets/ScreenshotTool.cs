using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotTool : MonoBehaviour
{
	public bool state = false;
	private void Update ()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			ScreenCapture.CaptureScreenshot("C:/Users/jules/OneDrive/Bureau/Screenshots/HelloThere.png", 5);
		}

		if (Input.GetKeyDown(KeyCode.G))
		{
			foreach (EnemyBehaviour b in FindObjectsOfType<EnemyBehaviour>())
			{
				state = !state;
				if (state)
				{
					b.ChangeState(EnemyState.Deploying);
				} else
				{
					b.ChangeState(EnemyState.Hidden);
				}
			}
		}
	}
}
