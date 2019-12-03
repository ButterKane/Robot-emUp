using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSniperBehaviour : TurretBehaviour
{
    public Vector2 minMaxFollowingAimingCubeScale;

    public override void LaunchProjectile()
    {
        base.LaunchProjectile();
        spawnedBullet.GetComponent<TurretSniperBullet>().target = focusedPlayerTransform;
    }

    public override void Update()
    {
        base.Update();
        UpdateAimingCubeState();
    }

    public override void AttackingUpdateState()
    {
        switch (attackState)
        {
            //-------------------------------------------------------
            case TurretAttackState.Anticipation:
                if (focusedPlayerTransform != null)
                {
                    RotateTowardsPlayerAndHisForward();
                }
                anticipationTime -= Time.deltaTime;
                if (anticipationTime <= 0)
                {
                    attackState = TurretAttackState.Attack;
                    Animator.SetTrigger("AttackTrigger");
                }
                break;
            //-------------------------------------------------------
            case TurretAttackState.Attack:
                if (focusedPlayerTransform != null)
                {
                    RotateTowardsPlayerAndHisForward();
                }
                break;
            //-------------------------------------------------------
            case TurretAttackState.Rest:
                restTime -= Time.deltaTime;
                if (restTime <= 0)
                {
                    Animator.SetTrigger("FromRestToIdleTrigger");
                    ChangingState(TurretState.Idle);
                }

                if (aimingCubeState != AimingCubeState.NotVisible)
                {
                    ChangeAimingCubeState(AimingCubeState.NotVisible);
                }

                if (focusedPlayerTransform != null)
                {
                    RotateTowardsPlayerAndHisForward();
                }
                break;
             //-------------------------------------------------------
        }

        //Adapt aimCube Scale and Position
        RaycastHit hit;
        if (Physics.Raycast(_self.position, _self.forward, out hit, 50, layersToCheckToScale))
        {
            aimingCubeTransform.localScale = new Vector3(aimingCubeTransform.localScale.x, aimingCubeTransform.localScale.y, Vector3.Distance(_self.position, hit.point));
            aimingCubeTransform.position = _self.position + _self.up * .5f + (aimingCubeTransform.localScale.z / 2 * _self.forward);
        }
    }

    void UpdateAimingCubeState()
    {
        switch (aimingCubeState)
        {
            case AimingCubeState.Following:
                float randomFloat = Random.Range(0f, 1f);
                print(randomFloat);
                if (randomFloat>0.5f)
                {
                    aimingCubeTransform.localScale = new Vector3(minMaxFollowingAimingCubeScale.x, minMaxFollowingAimingCubeScale.x, aimingCubeTransform.localScale.z);
                }
                else
                {
                    aimingCubeTransform.localScale = new Vector3(minMaxFollowingAimingCubeScale.y, minMaxFollowingAimingCubeScale.y, aimingCubeTransform.localScale.z);
                }
                break;
            case AimingCubeState.Locking:
                break;
            case AimingCubeState.NotVisible:
                break;
        }
    }
}
