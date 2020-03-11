﻿using System.Collections;
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

    public override void Shoot()
    {
        Vector3 i_spawnPosition;
        i_spawnPosition = bulletSpawn.position;
        spawnedBullet = Instantiate(bulletPrefab, i_spawnPosition, Quaternion.LookRotation(modelPivot.forward));
        spawnedBullet.GetComponent<TurretSniperBullet>().target = focusedPawnController.transform;
        spawnedBullet.GetComponent<TurretSniperBullet>().spawnParent = transform;
    }

    public override void Die()
    {
        if (Random.Range(0f, 1f) <= coreDropChances)
        {
            DropCore();
        }

        Destroy(gameObject);
    }

    protected override void Update()
    {
        base.Update();
        UpdateAimingCubeState();
    }

    public override void ExitTurretState()
    {
        switch (turretState)
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

    public override void EnterTurretState()
    {
        //print(State);
        switch (turretState)
        {
            case TurretState.Hiding:
                animator.SetTrigger("HidingTrigger");
                break;
            case TurretState.GettingOutOfGround:
                animator.SetTrigger("GettingOutOfGroundTrigger");
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                //VARIABLES GAMEPLAY------------------
                attackState = TurretAttackState.Anticipation;
                animator.SetTrigger("AnticipationTrigger");
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

    public override void AttackingUpdateState()
    {
		bool i_aimAtPlayer = false;
        
		//Adapt aimCube Scale and Position
		RaycastHit hit = default;
		RaycastHit[] hits = Physics.RaycastAll(aimingRedDotTransform.position, aimingRedDotTransform.forward, 500);
		System.Array.Sort(hits, ( x, y ) => x.distance.CompareTo(y.distance));
		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Player")) {
				if (hits[i].collider.tag != "Player")
				{
					continue;
				} else
				{
					i_aimAtPlayer = true;
					hit = hits[i];
					aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Vector3.Distance(aimingRedDotTransform.position, hit.point));
					break;
				}
			}
			if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
			{
				hit = hits[i];
				aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Vector3.Distance(aimingRedDotTransform.position, hit.point));
				break;
			}
			hit = hits[i];
			aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Vector3.Distance(aimingRedDotTransform.position, hit.point));
        }

        //Adapt PlayerFXRenderer

        switch (attackState)
        {
            //-------------------------------------------------------
            case TurretAttackState.Anticipation:
                //ADAPT FXs
                if (i_aimAtPlayer)
                {
                    aimingAtPlayerFXTransform.position = focusedPawnController.transform.position + Vector3.up * 0.1f;
					aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(Vector3.up);
					aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnPlayer;
                }
                else
                {
                    aimingAtPlayerFXTransform.position = hit.point + hit.normal * 0.2f;
                    aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(hit.normal);
					aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnWall;
                }

                //ROTATE TOWARDS PLAYER
                if (focusedPawnController != null)
                {
                    RotateTowardsPlayerAndHisForward();
                    // rotating an object here works, so the method must be malfunctionning or the mainbody rotation is touched somewhere else
                }

                //TRANSITION TO OTHER STATE
                anticipationTime -= Time.deltaTime;
                aimingAtPlayerFXRenderer.material.SetFloat("_CircleThickness", Mathf.Lerp(startAimingFXCircleThickness, 1, 1 - (anticipationTime/maxAnticipationTime)));
                aimingAtPlayerFXTransform.localScale *= Mathf.Lerp(1, endAimingFXScaleMultiplier, 1 - (anticipationTime / maxAnticipationTime));

                if (anticipationTime <= 0)
                {
                    attackState = TurretAttackState.Attack;
                    animator.SetTrigger("AttackTrigger");
                    // reset FX variables before attacking ! --------------------------------------------------------------------------
                    aimingAtPlayerFXRenderer.material.SetFloat("_EmissiveMultiplier", 10);
                    aimingAtPlayerFXRenderer.material.SetColor("_EmissiveColor", Color.yellow);
                    aimingAtPlayerFXRenderer.material.SetFloat("_CircleThickness", startAimingFXCircleThickness);
                }
                break;
            //-------------------------------------------------------
            case TurretAttackState.Attack:
                //ADAPT FXs----------------------------------
                if (i_aimAtPlayer)
                {
                    aimingAtPlayerFXTransform.position = focusedPawnController.transform.position + Vector3.up * 0.1f;
					aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(Vector3.up);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnPlayer;
                }
                else
                {
                    aimingAtPlayerFXTransform.position = hit.point + hit.normal * 0.2f;
					aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(hit.normal);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnWall;
                }
                //ROTATE TOWARDS PLAYER-------------------------------------
                if (focusedPawnController != null)
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
                    animator.SetTrigger("FromRestToIdleTrigger");
                    ChangingTurretState(TurretState.Idle);
                }

                if (aimingRedDotState != AimingRedDotState.NotVisible)
                {
                    ChangeAimingRedDotState(AimingRedDotState.NotVisible);
                }

                if (focusedPawnController != null)
                {
                    RotateTowardsPlayerAndHisForward();
                }
                break;
             //-------------------------------------------------------
        }
    }

    void UpdateAimingCubeState()
    {
        switch (aimingRedDotState)
        {
            case AimingRedDotState.Following:
                float i_randomFloat = Random.Range(0f, 1f);
                if (i_randomFloat>0.5f)
                {
                    aimingRedDotTransform.localScale = new Vector3(minMaxFollowingAimingCubeScale.x, minMaxFollowingAimingCubeScale.x, aimingRedDotTransform.localScale.z);
                }
                else
                {
                    aimingRedDotTransform.localScale = new Vector3(minMaxFollowingAimingCubeScale.y, minMaxFollowingAimingCubeScale.y, aimingRedDotTransform.localScale.z);
                }
                break;
            case AimingRedDotState.Locking:
                break;
            case AimingRedDotState.NotVisible:
                break;
        }
    }
}
