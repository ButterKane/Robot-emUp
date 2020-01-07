using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : MonoBehaviour
{
    private Transform self;
    public float distanceToOtherPlayer = 5f;
    public float orbitSpeed = 25f;
    public float adjustDistanceSpeed = 3f;
    public Rigidbody rb;

    private void Awake()
    {
        self = transform;
        rb = this.gameObject.GetComponent<Rigidbody>();
    }
    public void MoveAroundPlayer(Transform _otherPlayer)
    {
        Vector3 internal_targetPosition = _otherPlayer.position;

        self.LookAt(internal_targetPosition);

        float internal_distanceToTarget = (internal_targetPosition - self.position).magnitude;

        Vector3 internal_moveDirection = Vector3.zero;
        rb.velocity = Vector3.zero;

        if (internal_distanceToTarget < distanceToOtherPlayer - 0.5f)
        {
            rb.AddForce(-self.forward * adjustDistanceSpeed * ((distanceToOtherPlayer*3) - internal_distanceToTarget) * Time.deltaTime, ForceMode.VelocityChange);
        }
        else if (internal_distanceToTarget > distanceToOtherPlayer + 0.5f)
        {
            rb.AddForce(self.forward * adjustDistanceSpeed * internal_distanceToTarget * Time.deltaTime, ForceMode.VelocityChange);
        }
        
        rb.AddForce(self.right * orbitSpeed * Time.deltaTime, ForceMode.VelocityChange);

    }
}
