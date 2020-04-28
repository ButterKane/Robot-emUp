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
    public float startAimingFXCircleThickness;

    [Range(0, 1)] public float rotationSpeedReductionRatio;

    [Header("Laser Repulsion")]
    public float repulseCircleRadius = 2;
    public float repulseCircleStrength = 2;
    public float playerSpeedReductionCoef = 0.5f;

    [Header("Laser Variables")]
    public GameObject FXChargingParticlesPrefab;
    public GameObject FXChargingMainLaserPrefab;
    public int damagePerSecond = 50;
    public float laserMaxLength = 10f;
    public float shootingLaserMaxTime = 3;
    private float shootingLaserTimeProgression;
    [Range(0, 1)] public float whenToTriggerLaserReduction = 0.8f;
    private float timeToTriggerLaserReduction;
    public AnimationCurve reducingOfLaserWidth;

    private IEnumerator laserShootingCoroutine;
    private GameObject FXChargingParticlesInstance;
    private ParticleSystem[] chargingPartSystList;
    private GameObject FXChargingMainLaserInstance;
    private ParticleSystem[] chargingMainSystList;
    private LaserSniper spawnedLaserScript;
    private MeshRenderer laserRenderer;
    private float normalLaserWidth;

    protected override void Start()
    {
        base.Start();
        eventOnDeath = "event.TurretLaserDeath";
        CreateChargingFxs();
        isBumpable = false;
        CreateProjectile();
        ChangingTurretState(TurretState.Hidden);
    }

    protected override void Update()
    {
        base.Update();
        UpdateAimingRedDotState();
    }

    #region Change Turret State
    protected override void EnterTurretState()
    {
        switch (turretState)
        {
            case TurretState.Hiding:
                animator.SetTrigger("HidingTrigger");
                if (baseAnimator != null) { baseAnimator.SetTrigger("HidingTrigger");}
                break;
            case TurretState.GettingOutOfGround:
                animator.SetTrigger("GettingOutOfGroundTrigger");
                if (baseAnimator != null) { baseAnimator.SetTrigger("GettingOutOfGroundTrigger");}
                ChangingTurretAttackState(TurretAttackState.NotAttacking);
                break;
            case TurretState.Hidden:
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                ChangeAimingRedDotState(AimingRedDotState.Following);
                ChangingTurretAttackState(TurretAttackState.Anticipation);
                break;
            case TurretState.Idle:
                timeBetweenCheck = 0;
                break;
        }
    }

    protected override void ExitTurretState()
    {
        switch (turretState)
        {
            case TurretState.Hiding:
                break;
            case TurretState.GettingOutOfGround:
                animator.ResetTrigger("GettingOutOfGroundTrigger");
                break;
            case TurretState.Hidden:
                animator.SetTrigger("GettingOutOfGroundTrigger");
                if (baseAnimator != null) { baseAnimator.SetTrigger("GettingOutOfGroundTrigger"); }
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                break;
            case TurretState.Idle:
                break;
        }
    }
    #endregion

    #region Change Turret Attack State
    protected override void EnterTurretAttackState()
    {
        switch (attackState)
        {
            case TurretAttackState.Anticipation:
                //VARIABLES GAMEPLAY------------------
                animator.SetTrigger("AnticipationTrigger");
                currentAnticipationTime = attackValues.maxAnticipationTime;
                restTime = maxRestTime + Random.Range(-randomRangeRestTime, randomRangeRestTime);

                //VARIABLES FXs--------------------------------------
                if (FXChargingParticlesInstance & FXChargingMainLaserInstance) { PlayChargingFxs(); } // Play the Charging Fxs

                aimingAtPlayerFXRenderer.material.SetFloat("_AddToCompleteCircle", 1);
                aimingAtPlayerFXRenderer.material.SetFloat("_EmissiveMultiplier", 2);
                aimingAtPlayerFXRenderer.material.SetColor("_EmissiveColor", Color.red);
                aimingAtPlayerFXRenderer.material.SetFloat("_CircleThickness", startAimingFXCircleThickness);

                laserShootingCoroutine = null;
                break;
            case TurretAttackState.Attack:

                animator.SetTrigger("StartLaserTrigger");
                attackDuration = shootingLaserMaxTime;
                Shoot();
                break;
            case TurretAttackState.Rest:
                ResetValuesAtEndOfAttack();
                animator.SetTrigger("EndOfAttackTrigger");
                restTime = maxRestTime;
                break;
            case TurretAttackState.NotAttacking:
                break;
        }
    }

    protected override void ExitTurretAttackState()
    {
        switch (attackState)
        {
            case TurretAttackState.Anticipation:
                //ADAPT FXs----------------------------------
                aimingAtPlayerFXTransform.gameObject.SetActive(false);

                if (FXChargingParticlesInstance & FXChargingMainLaserInstance) { StopChargingFxs(); }

                ChangeAimingRedDotState(AimingRedDotState.NotVisible);
                break;
            case TurretAttackState.Attack:
                ChangeAimingRedDotState(AimingRedDotState.NotVisible);
                break;
            case TurretAttackState.Rest:
                animator.SetTrigger("FromRestToIdleTrigger");
                break;
            case TurretAttackState.NotAttacking:
                break;
        }
    }

    protected override void AttackingUpdateState()
    {
        switch (attackState)
        {
            //-------------------------------------------------------
            case TurretAttackState.Anticipation:
                // Update Red Dot -------------------------
                RaycastHit i_hit;
                bool i_isAimingAtPLayer;
                RedDotOfAnticipation(out i_hit, out i_isAimingAtPLayer);

                // Circle on player -----------------------
                aimingAtPlayerFXTransform.gameObject.SetActive(true);
                if (i_isAimingAtPLayer)
                {
                    aimingAtPlayerFXTransform.position = focusedPawnController.transform.position + Vector3.up * 0.1f;
                    aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(Vector3.up);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnPlayer;
                }
                else
                {
                    aimingAtPlayerFXTransform.position = i_hit.point + i_hit.normal * 0.2f;
                    aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(i_hit.normal);
                    aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnWall;
                }

                aimingAtPlayerFXRenderer.material.SetFloat("_CircleThickness", Mathf.Lerp(startAimingFXCircleThickness, 1, 1 - (currentAnticipationTime / attackValues.maxAnticipationTime)));
                aimingAtPlayerFXTransform.localScale *= Mathf.Lerp(1, endAimingFXScaleMultiplier, 1 - (currentAnticipationTime / attackValues.maxAnticipationTime));

                // Charging energy ball in front of turret
                FXChargingMainLaserInstance.transform.localScale = Vector3.one * Mathf.Clamp(attackValues.maxAnticipationTime / currentAnticipationTime + 0.01f, 1, 6);

                if (currentAnticipationTime < attackValues.maxAnticipationTime * 0.2)
                {
                    ParticleSystem[] i_systems = FXChargingParticlesInstance.GetComponentsInChildren<ParticleSystem>();
                    foreach (var system in i_systems)
                    {
                        system.Stop();
                    }
                }

                //ROTATE TOWARDS PLAYER
                if (focusedPawnController != null)
                {
                    RotateTowardsPlayerPosition();
                }

                //TRANSITION TO OTHER STATE
                currentAnticipationTime -= Time.deltaTime;

                if (currentAnticipationTime <= 0)
                {
                    ChangingTurretAttackState(TurretAttackState.Attack);
                }
                break;
            //-------------------------------------------------------
            case TurretAttackState.Attack:
                aimingRedDotTransform.localScale = Vector3.zero;
                //ROTATE TOWARDS PLAYER-------------------------------------
                if (focusedPawnController != null)
                {
                    RotateTowardsPlayerPosition(rotationSpeedReductionRatio);   // rotate slower toward player
                }
                else
                {
                    if (attackDuration > 0)
                    {
                        ChangingTurretAttackState(TurretAttackState.Rest);
                    }
                }

                //TRANSITION TO OTHER STATE
                attackDuration -= Time.deltaTime;

                if (attackDuration < 0)
                {
                    ChangingTurretAttackState(TurretAttackState.Rest);
                }
                break;
            //-------------------------------------------------------
            case TurretAttackState.Rest:
                restTime -= Time.deltaTime;
                if (restTime <= 0)
                {
                    ChangingTurretAttackState(TurretAttackState.NotAttacking);
                    ChangingTurretState(TurretState.Idle);
                }
                break;
        }
    }
    #endregion

    #region Public methods
    new public void Shoot()
    {
        if (laserShootingCoroutine == null)
        {
            laserShootingCoroutine = ShootingLaser_C();
            StartCoroutine(laserShootingCoroutine);
        }
    }
    #endregion

    #region Private and protected methods
    protected void RedDotOfAnticipation(out RaycastHit outHit, out bool outAimingAtPlayer)
    {
        bool i_aimAtPlayer = false;
        //Adapt aimCube Scale and Position
        RaycastHit hit = default;
        RaycastHit[] hits = Physics.RaycastAll(aimingRedDotTransform.position, aimingRedDotTransform.forward, 500);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (hits[i].collider.tag != "Player")
                {
                    continue;
                }
                else
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

        outAimingAtPlayer = i_aimAtPlayer;
        outHit = hit;
    }

    private void CreateChargingFxs()
    {
        FXChargingParticlesInstance = Instantiate(FXChargingParticlesPrefab, bulletSpawn.position, Quaternion.identity, modelPivot);
        FXChargingMainLaserInstance = Instantiate(FXChargingMainLaserPrefab, bulletSpawn.position, Quaternion.identity, modelPivot);   // has a 0.1sec start delay to make it look like the charging particles created it
        chargingPartSystList = FXChargingParticlesInstance.GetComponentsInChildren<ParticleSystem>();
        chargingMainSystList = FXChargingMainLaserInstance.GetComponentsInChildren<ParticleSystem>();
    }

    private void PlayChargingFxs()
    {
        foreach (ParticleSystem PS in chargingPartSystList)
        {
            PS.Play();
        }
        foreach (ParticleSystem PS in chargingMainSystList)
        {
            PS.Play();
        }
    }

    private void StopChargingFxs()
    {
        foreach (ParticleSystem PS in chargingPartSystList)
        {
            PS.Stop();
        }
        foreach (ParticleSystem PS in chargingMainSystList)
        {
            PS.Stop();
        }
    }

    // Make RedDot flicker
    void UpdateAimingRedDotState()
    {
        switch (aimingRedDotState)
        {
            case AimingRedDotState.Following:
                float i_randomFloat = Random.Range(0f, 1f);
                if (i_randomFloat > 0.5f)
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
        shootingLaserTimeProgression = timeToTriggerLaserReduction;
    }
    #endregion

    #region Overridden methods
    public override void ResetValuesAtEndOfAttack()
    {
        spawnedLaserScript.laserWidth = normalLaserWidth;
        aimingAtPlayerFXRenderer.material.SetFloat("_AddToCompleteCircle", 0);
        animator.ResetTrigger("StartLaserTrigger");
        animator.ResetTrigger("AnticipationTrigger");
        animator.ResetTrigger("FromRestToIdleTrigger");
    }

    protected override void AbortAttack()
    {
        ChangingTurretAttackState(TurretAttackState.Rest);
    }

    protected override void CreateProjectile()
    {
        if (spawnedBullet != null)
        {
            Destroy(spawnedBullet);
            spawnedBullet = null;
        }
        spawnedBullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(modelPivot.forward), modelPivot);
        spawnedLaserScript = spawnedBullet.GetComponent<LaserSniper>();
        spawnedLaserScript.enemyScript = this;
        spawnedLaserScript.spawnParent = transform;
        laserRenderer = spawnedLaserScript.laserRenderer;
        normalLaserWidth = spawnedLaserScript.laserWidth;
        spawnedBullet.gameObject.SetActive(false);
    }
    #endregion

    #region  Coroutines
    private IEnumerator ShootingLaser_C()
    {
        spawnedLaserScript.isLaserActive = true;

        spawnedBullet.transform.position = bulletSpawn.position;
        spawnedBullet.SetActive(true);

        shootingLaserTimeProgression = shootingLaserMaxTime;
        timeToTriggerLaserReduction = shootingLaserMaxTime - (whenToTriggerLaserReduction * shootingLaserMaxTime);

        while (shootingLaserTimeProgression > 0)
        {
            if (shootingLaserTimeProgression < timeToTriggerLaserReduction)
            {
                float reducingLaserFactor = reducingOfLaserWidth.Evaluate((timeToTriggerLaserReduction - shootingLaserTimeProgression) / (timeToTriggerLaserReduction));
                if (spawnedLaserScript != null)
                {
                    spawnedLaserScript.isLaserActive = false;
                    laserRenderer = spawnedLaserScript.laserRenderer;
                }

                spawnedLaserScript.laserWidth = normalLaserWidth * reducingLaserFactor;
            }
            shootingLaserTimeProgression -= Time.deltaTime;
            yield return null;
        }

        if (spawnedBullet != null)
        {
            spawnedLaserScript.isLaserActive = false;
            spawnedBullet.SetActive(false);
            spawnedBullet.transform.localScale = new Vector3(spawnedBullet.transform.localScale.x, spawnedBullet.transform.localScale.y, 0);
        }
    }
    #endregion 
}
