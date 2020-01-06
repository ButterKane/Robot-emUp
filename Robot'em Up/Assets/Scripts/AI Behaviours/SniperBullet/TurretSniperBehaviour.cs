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

    public override void LaunchProjectile()
    {
        Vector3 internal_spawnPosition;
        internal_spawnPosition = bulletSpawn.position;
        spawnedBullet = Instantiate(bulletPrefab, internal_spawnPosition, Quaternion.LookRotation(transform.forward));
        spawnedBullet.GetComponent<TurretSniperBullet>().target = focusedPlayerTransform;
        spawnedBullet.GetComponent<TurretSniperBullet>().spawnParent = self;

        FeedbackManager.SendFeedback("event.SniperTurretAttack", this);
        SoundManager.PlaySound("SniperTurretAttack", transform.position);
    }

    public override void Die()
    {
        GameObject internal_deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        internal_deathParticle.transform.localScale *= deathParticleScale;
        Destroy(internal_deathParticle, 1.5f);

        if (Random.Range(0f, 1f) <= coreDropChances)
        {
            DropCore();
        }

        FeedbackManager.SendFeedback("event.SniperTurretDeath", this);
        SoundManager.PlaySound("SniperTurretDeath", transform.position);

        Destroy(gameObject);
    }

    public override void Update()
    {
        base.Update();
        UpdateAimingCubeState();
    }

    public override void ExitState()
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

    public override void EnterState()
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
        bool internal_aimAtPlayer;
        
        if(Physics.Raycast(self.position, self.forward, Vector3.Distance(self.position, focusedPlayerTransform.position), layersToCheckToScale))
        {
            internal_aimAtPlayer = false;
        }
        else
        {
            internal_aimAtPlayer = true;
        }
        //Adapt aimCube Scale and Position
        RaycastHit hit;
        if (Physics.Raycast(self.position, self.forward, out hit, 50, layersToCheckToScale))
        {
            aimingCubeTransform.localScale = new Vector3(aimingCubeTransform.localScale.x, aimingCubeTransform.localScale.y, Vector3.Distance(self.position, hit.point));
            aimingCubeTransform.position = self.position + self.up * .5f + (aimingCubeTransform.localScale.z / 2 * self.forward);
        }

        //Adapt PlayerFXRenderer

        switch (attackState)
        {
            //-------------------------------------------------------
            case TurretAttackState.Anticipation:
                //ADAPT FXs
                if (internal_aimAtPlayer)
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
                if (internal_aimAtPlayer)
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
                    animator.SetTrigger("FromRestToIdleTrigger");
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
                float internal_randomFloat = Random.Range(0f, 1f);
                if (internal_randomFloat>0.5f)
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
