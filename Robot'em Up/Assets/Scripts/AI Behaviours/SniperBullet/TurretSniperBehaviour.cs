using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSniperBehaviour : TurretBehaviour
{
    public Vector2 minMaxFollowingAimingCubeScale;
    public Transform aimingAtPlayerFXTransform;
    public Renderer aimingAtPlayerFXRenderer;
    public Vector3 aimingAtPlayerFXScaleOnWall;
    public Vector3 aimingAtPlayerFXScaleOnPlayer;
    public float endAimingFXScaleMultiplier;
    public float startAimingFXCircleThickness;
    bool playerInLineOfSight;
    public float timeForBulletToReach;

    public override void LaunchProjectile()
    {
        GameObject _sniperBulletRef = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.LookRotation(bulletSpawn.forward));

        if (playerInLineOfSight)
        {
            _sniperBulletRef.GetComponent<TurretSniperBullet>().target = focusedPlayerTransform;
            _sniperBulletRef.GetComponent<TurretSniperBullet>().spawnParent = _self;
            _sniperBulletRef.GetComponent<TurretSniperBullet>().isAimingPlayer = true;
        }
        else
        {
            _sniperBulletRef.GetComponent<TurretSniperBullet>().target = focusedPlayerTransform;
            _sniperBulletRef.GetComponent<TurretSniperBullet>().spawnParent = _self;
            _sniperBulletRef.GetComponent<TurretSniperBullet>().isAimingPlayer = false;
        }
    }

    public override void Update()
    {
        base.Update();
        UpdateAimingCubeState();
    }

    public override void ExitState()
    {
        switch (State)
        {
            case TurretState.Hiding:
                break;
            case TurretState.GettingOutOfGround:
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                //VARIABLES FXs--------------------------------------
                aimingAtPlayerFXRenderer.material.SetFloat("_EmissiveMultiplier", 2);
                aimingAtPlayerFXRenderer.material.SetColor("_EmissiveColor", Color.red);
                aimingAtPlayerFXRenderer.material.SetFloat("_CircleThickness", startAimingFXCircleThickness);
                break;
            case TurretState.Idle:
                break;
        }
    }

    public override void EnterState()
    {
        //print(State);
        switch (State)
        {
            case TurretState.Hiding:
                Animator.SetTrigger("HidingTrigger");
                break;
            case TurretState.GettingOutOfGround:
                Animator.SetTrigger("GettingOutOfGroundTrigger");
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                //VARIABLES GAMEPLAY------------------
                attackState = TurretAttackState.Anticipation;
                Animator.SetTrigger("AnticipationTrigger");
                anticipationTime = maxAnticipationTime;
                restTime = maxRestTime + Random.Range(-randomRangeRestTime, randomRangeRestTime);
                //VARIABLES FXs--------------------------------------
                aimingAtPlayerFXRenderer.material.SetFloat("_AddToCompleteCircle", 1);
                break;
            case TurretState.Idle:
                timeBetweenCheck = 0;
                break;
        }
    }

    void UpdatePlayerInLineOfSight()
    {
        if (Physics.Raycast(_self.position, _self.forward, Vector3.Distance(_self.position, focusedPlayerTransform.position), layersToCheckToScale))
        {
            playerInLineOfSight = false;
        }
        else
        {
            playerInLineOfSight = true;
        }
    }

    public override void AttackingUpdateState()
    {
        UpdatePlayerInLineOfSight();

        //Adapt aimCube Scale and Position
        RaycastHit hit;
        if (Physics.Raycast(_self.position, _self.forward, out hit, 50, layersToCheckToScale))
        {
            aimingCubeTransform.localScale = new Vector3(aimingCubeTransform.localScale.x, aimingCubeTransform.localScale.y, Vector3.Distance(_self.position, hit.point));
            aimingCubeTransform.position = _self.position + _self.up * .5f + (aimingCubeTransform.localScale.z / 2 * _self.forward);
        }

        //Adapt PlayerFXRenderer

        switch (attackState)
        {
            //-------------------------------------------------------
            case TurretAttackState.Anticipation:
                //ADAPT FXs
                if (playerInLineOfSight)
                {
                    aimingAtPlayerFXTransform.position = focusedPlayerTransform.position;
                    aimingAtPlayerFXTransform.rotation = Quaternion.Euler(90, 0, 0);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnPlayer;
                }
                else
                {
                    aimingAtPlayerFXTransform.position = new Vector3(hit.point.x, aimingCubeTransform.position.y, hit.point.z) + hit.normal * 0.2f;
                    aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(180, 0, 0);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnWall;
                }

                //ROTATE TOWARDS PLAYER
                if (focusedPlayerTransform != null)
                {
                    RotateTowardsPlayerAndHisForward();
                }

                //TRANSITION TO OTHER STATE
                anticipationTime -= Time.deltaTime;
                aimingAtPlayerFXRenderer.material.SetFloat("_CircleThickness", Mathf.Lerp(startAimingFXCircleThickness, 1, 1 - (anticipationTime/maxAnticipationTime)));
                aimingAtPlayerFXTransform.localScale *= Mathf.Lerp(1, endAimingFXScaleMultiplier, 1 - (anticipationTime / maxAnticipationTime));

                if (anticipationTime <= 0)
                {
                    attackState = TurretAttackState.Attack;
                    Animator.SetTrigger("AttackTrigger");
                    // reset FX variables before attacking ! --------------------------------------------------------------------------
                    aimingAtPlayerFXRenderer.material.SetFloat("_EmissiveMultiplier", 10);
                    aimingAtPlayerFXRenderer.material.SetColor("_EmissiveColor", Color.yellow);
                    aimingAtPlayerFXRenderer.material.SetFloat("_CircleThickness", startAimingFXCircleThickness);
                }
                break;
            //-------------------------------------------------------
            case TurretAttackState.Attack:
                //ADAPT FXs----------------------------------
                if (playerInLineOfSight)
                {
                    aimingAtPlayerFXTransform.position = focusedPlayerTransform.position;
                    aimingAtPlayerFXTransform.rotation = Quaternion.Euler(90, 0, 0);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnPlayer;
                }
                else
                {
                    aimingAtPlayerFXTransform.position = new Vector3(hit.point.x, aimingCubeTransform.position.y, hit.point.z) + hit.normal * 0.2f;
                    aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(180, 0, 0);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnWall;
                }
                //ROTATE TOWARDS PLAYER-------------------------------------
                if (focusedPlayerTransform != null)
                {
                    RotateTowardsPlayerAndHisForward();
                }
                break;
            //-------------------------------------------------------
            case TurretAttackState.Rest:
                restTime -= Time.deltaTime;
                aimingAtPlayerFXRenderer.material.SetFloat("_AddToCompleteCircle", 0);
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
    }

    void UpdateAimingCubeState()
    {
        switch (aimingCubeState)
        {
            case AimingCubeState.Following:
                float randomFloat = Random.Range(0f, 1f);
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
