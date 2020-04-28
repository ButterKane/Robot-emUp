using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : NarrativeInteractiveElements
{
    public Rigidbody CameraRB;
    public float forwardForceThrow;
    public float downForceThrow;
    public Transform cameraPivot;
    public Quaternion cameraPivotDefaultRotation;

    public override void Start()
    {
        base.Start();
        cameraPivotDefaultRotation = cameraPivot.rotation;
    }

    public override void Break()
    {
        base.Break();
        CameraRB.transform.parent = null;
        CameraRB.isKinematic = false;
        CameraRB.AddForce(transform.forward * forwardForceThrow + -transform.up * downForceThrow);
    }

    private void Update()
    {
        if (possessed)
        {
            FollowPlayersMiddlePoint();
        }
    }

    void FollowPlayersMiddlePoint()
    {
        Vector3 closestPlayerDirection = GetCloserPlayer().position - cameraPivot.position;

        if (Vector3.Angle(transform.forward, closestPlayerDirection) < Vector3.Angle(-transform.forward, closestPlayerDirection))
        {
            cameraPivot.rotation = Quaternion.Lerp(cameraPivot.rotation, Quaternion.LookRotation(closestPlayerDirection), 0.08f);
        }
        else
        {
            cameraPivot.rotation = Quaternion.Lerp(cameraPivot.rotation, cameraPivotDefaultRotation, 0.08f);
        }
    }

}
