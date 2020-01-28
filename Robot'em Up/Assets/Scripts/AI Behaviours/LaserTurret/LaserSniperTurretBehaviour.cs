using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSniperTurretBehaviour : TurretBehaviour
{
    public Vector2 minMaxFollowingAimingRedDotScale;
    public Transform aimingAtPlayerFXTransform;
    public Renderer aimingAtPlayerFXRenderer;
    public Vector3 aimingAtPlayerFXScaleOnWall;
    public Vector3 aimingAtPlayerFXScaleOnPlayer;
    public float endAimingFXScaleMultiplier;

    [Range(0, 1)] public float rotationSpeedReductionRatio;
    
    public float repulseCircleRadius;
    public float repulseCircleStrength;
    public float finalRepulseCircleRadius;
    public float finalRepulseCircleStrength;

    public int damagePerSecond = 50;
    public float laserMaxLength = 10f;
    public float shootingLaserMaxTime = 3;
    private float shootingLaserTimeProgression;
    [Range(0, 1)] public float whenToTriggerLaserReduction = 0.8f;
    private float timeToTriggerLaserReduction;
    public AnimationCurve reducingOfLaserWidth;

    private IEnumerator LaserShootingCoroutine;



    new public void Shoot()
    {
        if (LaserShootingCoroutine == null)
        {
            LaserShootingCoroutine = ShootingLaser_C();
            StartCoroutine(ShootingLaser_C());
        }
    }


    public IEnumerator ShootingLaser_C()
    {
        LaserSniper i_instance = null;
        MeshRenderer i_laserRenderer = null;
        if (spawnedBullet == null)
        {
            Vector3 i_spawnPosition;
            i_spawnPosition = bulletSpawn.position;
            spawnedBullet = Instantiate(bulletPrefab, i_spawnPosition, Quaternion.LookRotation(transform.forward), transform);
            i_instance = spawnedBullet.GetComponent<LaserSniper>();
            i_instance.enemyScript = this;
            i_instance.target = focusedPlayer;
            i_instance.spawnParent = transform;
            shootingLaserTimeProgression = shootingLaserMaxTime;
            timeToTriggerLaserReduction = shootingLaserMaxTime - ( whenToTriggerLaserReduction * shootingLaserMaxTime); 
        }

        // IF nothing is touched
        //spawnedBullet.transform.localScale = new Vector3 (spawnedBullet.transform.localScale.x, spawnedBullet.transform.localScale.y, laserMaxLength);

        while (shootingLaserTimeProgression > 0)
        {
            if (shootingLaserTimeProgression < timeToTriggerLaserReduction)
            {
                float reducingLaserFactor = reducingOfLaserWidth.Evaluate((timeToTriggerLaserReduction - shootingLaserTimeProgression) / (timeToTriggerLaserReduction));
                if (i_instance != null)
                {
                    i_instance.isLaserActive = false;
                    i_laserRenderer = i_instance.laserRenderer;
                }
                i_laserRenderer.material.color = new Color(i_laserRenderer.material.color.r, i_laserRenderer.material.color.g, i_laserRenderer.material.color.b, i_laserRenderer.material.color.a * reducingLaserFactor);
                i_instance.laserWidth = i_instance.laserWidth * reducingLaserFactor;
            }
            shootingLaserTimeProgression -= Time.deltaTime;
            yield return null;
        }
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
        //UpdateAimingRedDotState();
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
            aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Vector3.Distance(transform.position, hit.point));
            aimingRedDotTransform.position = transform.position + transform.up * .5f + (aimingRedDotTransform.localScale.z / 2 * transform.forward);
        }

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
                    aimingAtPlayerFXTransform.position = new Vector3(hit.point.x, aimingRedDotTransform.position.y, hit.point.z) + hit.normal * 0.2f;
                    aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(180, 0, 0);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnWall;
                }

                //ROTATE TOWARDS PLAYER
                if (focusedPlayer != null)
                {
                    RotateTowardsPlayerPosition();
                }

                //TRANSITION TO OTHER STATE
                anticipationTime -= Time.deltaTime;
                //aimingAtPlayerFXTransform.localScale *= Mathf.Lerp(1, endAimingFXScaleMultiplier, 1 - (anticipationTime / maxAnticipationTime));

                if (anticipationTime <= 0)
                {
                    attackState = TurretAttackState.Attack;
                    animator.SetTrigger("StartLaserTrigger");
                    attackDuration = shootingLaserMaxTime;
                    Shoot();
                    // reset FX variables before attacking ! --------------------------------------------------------------------------
                    //aimingAtPlayerFXRenderer.material.SetFloat("_EmissiveMultiplier", 10);
                    //aimingAtPlayerFXRenderer.material.SetColor("_EmissiveColor", Color.yellow);
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
                    aimingAtPlayerFXTransform.position = new Vector3(hit.point.x, aimingRedDotTransform.position.y, hit.point.z) + hit.normal * 0.2f;
                    aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(180, 0, 0);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnWall;
                }

                //ROTATE TOWARDS PLAYER-------------------------------------
                if (focusedPlayer != null)
                {
                    RotateTowardsPlayerPosition();
                }

                //TRANSITION TO OTHER STATE
                attackDuration -= Time.deltaTime;

                if (attackDuration < 0)
                {
                    StopLaser();
                    restTime = maxRestTime;
                    attackState = TurretAttackState.Rest;
                    animator.SetTrigger("EndOfAttackTrigger");
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

                if (aimingRedDotState != AimingRedDotState.NotVisible)
                {
                    ChangeAimingRedDotState(AimingRedDotState.NotVisible);
                }

                if (focusedPlayer != null)
                {
                    RotateTowardsPlayerPosition();
                }
                break;
        }
    }

    // Make RedDot flicker
    void UpdateAimingRedDotState()
    {
        switch (aimingRedDotState)
        {
            case AimingRedDotState.Following:
                float i_randomFloat = Random.Range(0f, 1f);
                if (i_randomFloat>0.5f)
                {
                    aimingRedDotTransform.localScale = new Vector3(minMaxFollowingAimingRedDotScale.x, minMaxFollowingAimingRedDotScale.x, aimingRedDotTransform.localScale.z);
                }
                else
                {
                    aimingRedDotTransform.localScale = new Vector3(minMaxFollowingAimingRedDotScale.y, minMaxFollowingAimingRedDotScale.y, aimingRedDotTransform.localScale.z);
                }
                break;
            case AimingRedDotState.Locking:
                break;
            case AimingRedDotState.NotVisible:
                break;
        }
    }

    void StopLaser()
    {
        if (LaserShootingCoroutine != null)
        {
            StopCoroutine(LaserShootingCoroutine);
            LaserShootingCoroutine = null;
        }
        if (spawnedBullet != null)
        {
            Destroy(spawnedBullet);
        }
    }
}
