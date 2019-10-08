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

    private void Start()
    {
        self = transform;
        rb = this.gameObject.GetComponent<Rigidbody>();
    }
    public void MoveAroundPlayer(Transform otherPlayer)
    {
        Vector3 targetPosition = otherPlayer.position;

        self.LookAt(targetPosition);

        float distanceToTarget = (targetPosition - self.position).magnitude;

        Vector3 moveDirection = Vector3.zero;
        rb.velocity = Vector3.zero;

        if (distanceToTarget < distanceToOtherPlayer - 0.5f)
        {
            rb.AddForce(-self.forward * adjustDistanceSpeed * ((distanceToOtherPlayer*3) - distanceToTarget) * Time.deltaTime, ForceMode.VelocityChange);
        }
        else if (distanceToTarget > distanceToOtherPlayer + 0.5f)
        {
            rb.AddForce(self.forward * adjustDistanceSpeed * distanceToTarget * Time.deltaTime, ForceMode.VelocityChange);
        }
        
        rb.AddForce(self.right * orbitSpeed * Time.deltaTime, ForceMode.VelocityChange);

    }
}
