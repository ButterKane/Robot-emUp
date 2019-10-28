using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRetriever : MonoBehaviour
{
    private Transform parent;
	private PassController passController;
	private PlayerController playerController;
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
    }
}
