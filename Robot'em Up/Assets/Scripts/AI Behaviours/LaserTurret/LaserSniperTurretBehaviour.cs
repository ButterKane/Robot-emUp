using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSniperTurretBehaviour : TurretBehaviour
{
    public Vector2 minMaxFollowingAimingCubeScale;
    public Transform aimingAtPlayerFXTransform;
    public Renderer aimingAtPlayerFXRenderer;
    public Vector3 aimingAtPlayerFXScaleOnWall;
    public Vector3 aimingAtPlayerFXScaleOnPlayer;
    public float endAimingFXScaleMultiplier;
    public float startAimingFXCircleThickness;
    
    
    
    public float repulseCircleRadius;
    public float repulseCircleStrength;
    public float finalRepulseCircleRadius;
    public float finalRepulseCircleStrength;

    public GameObject laserPrefab;
    public int damagePerSecond = 50;
    public float laserMaxLength = 10f;
    public float shootingLaserMaxTime = 3;
    private float shootingLaserTimeProgression;
    public float whenToTriggerLaserReduction;
    public AnimationCurve reducingOfLaserWidth;



    public override void Shoot()
    {
        StartCoroutine(ShootingLaser_C());
    }


    public IEnumerator ShootingLaser_C()
    {
        if (spawnedBullet == null)
        {
            Vector3 i_spawnPosition;
            i_spawnPosition = bulletSpawn.position;
            spawnedBullet = Instantiate(bulletPrefab, i_spawnPosition, Quaternion.LookRotation(transform.forward));
            LaserSniper i_instance = spawnedBullet.GetComponent<LaserSniper>();
            i_instance.enemyScript = this;
            i_instance.target = focusedPlayer;
            i_instance.spawnParent = transform;
            shootingLaserTimeProgression = shootingLaserMaxTime;
            whenToTriggerLaserReduction = shootingLaserMaxTime - (shootingLaserMaxTime * 0.8f); // will trigger reducing width when there's 20% of the max time left
        }
        
        // IF nothing is touched
        spawnedBullet.transform.localScale = new Vector3 (spawnedBullet.transform.localScale.x, spawnedBullet.transform.localScale.y, laserMaxLength);

        if (shootingLaserTimeProgression < whenToTriggerLaserReduction)
        {
            float reducingLaserFactor = reducingOfLaserWidth.Evaluate((shootingLaserTimeProgression - whenToTriggerLaserReduction) / (shootingLaserMaxTime - whenToTriggerLaserReduction));

            spawnedBullet.transform.localScale = new Vector3(spawnedBullet.transform.localScale.x * reducingLaserFactor, spawnedBullet.transform.localScale.y * reducingLaserFactor, spawnedBullet.transform.localScale.z);
        }

        shootingLaserTimeProgression -= Time.deltaTime;
        yield return null;
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
        bool i_aimAtPlayer = false;
        if (focusedPlayer)
        {
            if (Physics.Raycast(transform.position, transform.forward, Vector3.Distance(transform.position, focusedPlayer.position), layersToCheckToScale))
            {
                i_aimAtPlayer = false;
            }
            else
            {
                i_aimAtPlayer = true;
            }
        }
        //Adapt aimCube Scale and Position
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 50, layersToCheckToScale))
        {
            aimingCubeTransform.localScale = new Vector3(aimingCubeTransform.localScale.x, aimingCubeTransform.localScale.y, Vector3.Distance(transform.position, hit.point));
            aimingCubeTransform.position = transform.position + transform.up * .5f + (aimingCubeTransform.localScale.z / 2 * transform.forward);
        }

        //Adapt PlayerFXRenderer

        switch (attackState)
        {
            //-------------------------------------------------------
            case TurretAttackState.Anticipation:
                //ADAPT FXs
                if (i_aimAtPlayer)
                {
                    aimingAtPlayerFXTransform.position = focusedPlayer.position;
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
                if (focusedPlayer != null)
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
                if (i_aimAtPlayer)
                {
                    aimingAtPlayerFXTransform.position = focusedPlayer.position;
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
                if (focusedPlayer != null)
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

                if (focusedPlayer != null)
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
                float i_randomFloat = Random.Range(0f, 1f);
                if (i_randomFloat>0.5f)
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
