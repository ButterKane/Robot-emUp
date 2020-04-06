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
    private GameObject FXChargingMainLaserInstance;

    public GameObject redDotPrefab;


    protected override void Start()
    {
        base.Start();
        eventOnDeath = "event.TurretLaserDeath";
    }

    new public void Shoot()
    {
        if (laserShootingCoroutine == null)
        {
            laserShootingCoroutine = ShootingLaser_C();
            StartCoroutine(laserShootingCoroutine);
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
            spawnedBullet = Instantiate(bulletPrefab, i_spawnPosition, Quaternion.LookRotation(modelPivot.forward), modelPivot);
            i_instance = spawnedBullet.GetComponent<LaserSniper>();
            i_instance.enemyScript = this;
            i_instance.target = focusedPawnController.transform;
            i_instance.spawnParent = transform;
            shootingLaserTimeProgression = shootingLaserMaxTime;
            timeToTriggerLaserReduction = shootingLaserMaxTime - (whenToTriggerLaserReduction * shootingLaserMaxTime);

        }

        float normalLaserWidth = i_instance.laserWidth;

        while (shootingLaserTimeProgression > 0)
        {
            //Debug.Log("shoot laser coroutine progression: "+shootingLaserTimeProgression+" / " + timeToTriggerLaserReduction);
            if (shootingLaserTimeProgression < timeToTriggerLaserReduction)
            {
                float reducingLaserFactor = reducingOfLaserWidth.Evaluate((timeToTriggerLaserReduction - shootingLaserTimeProgression) / (timeToTriggerLaserReduction));
                if (i_instance != null)
                {
                    i_instance.isLaserActive = false;
                    i_laserRenderer = i_instance.laserRenderer;
                }
                //i_laserRenderer.material.color = new Color(i_laserRenderer.material.color.r, i_laserRenderer.material.color.g, i_laserRenderer.material.color.b, i_laserRenderer.material.color.a * reducingLaserFactor);
                
                i_instance.laserWidth = normalLaserWidth * reducingLaserFactor;
            }
            shootingLaserTimeProgression -= Time.deltaTime;
            yield return null;
        }

        if (spawnedBullet != null)
        {
            Destroy(spawnedBullet);
        }
    }

    public override void Die()
    {
        if (Random.Range(0f, 1f) <= deathValues.coreDropChances)
        {
            DropCore();
        }

        Destroy(gameObject);
    }

    protected override void Update()
    {
        base.Update();
        UpdateAimingRedDotState();
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
                break;
            case TurretState.Dying:
                break;
            case TurretState.Attacking:
                break;
            case TurretState.Idle:
                break;
        }
    }

    protected override void EnterTurretState()
    {
        //print(State);
        switch (turretState)
        {
            case TurretState.Hiding:
                animator.SetTrigger("HidingTrigger");
                break;
            case TurretState.GettingOutOfGround:
                animator.SetTrigger("GettingOutOfGroundTrigger");
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
                if (FXChargingParticlesInstance) { Destroy(FXChargingParticlesInstance); }
                if (FXChargingMainLaserInstance) { Destroy(FXChargingMainLaserInstance); }

                FXChargingParticlesInstance = Instantiate(FXChargingParticlesPrefab, bulletSpawn.position, Quaternion.identity, modelPivot);
                FXChargingMainLaserInstance = Instantiate(FXChargingMainLaserPrefab, bulletSpawn.position, Quaternion.identity, modelPivot);   // has a 0.1sec start delay to make it look like the charging particles created it

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

                if (FXChargingParticlesInstance) { Destroy(FXChargingParticlesInstance); }
                if (FXChargingMainLaserInstance) { Destroy(FXChargingMainLaserInstance); }

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

        switch (attackState)
        {
            //-------------------------------------------------------
            case TurretAttackState.Anticipation:
                // UPDATE FXS -------------------------------

                // Circle on player
                aimingAtPlayerFXTransform.gameObject.SetActive(true);
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

    public override void ResetValuesAtEndOfAttack() 
    {
        aimingAtPlayerFXRenderer.material.SetFloat("_AddToCompleteCircle", 0);
        animator.ResetTrigger("StartLaserTrigger");
        animator.ResetTrigger("AnticipationTrigger");
        animator.ResetTrigger("FromRestToIdleTrigger");
    }

    protected override void AbortAttack()
    {
        ChangingTurretAttackState(TurretAttackState.Rest);
    }

    IEnumerator UpdateAnticipationRedDot_C() // WIP, NOT FINISHED, MUST BE FINISHED
    {
        List<GameObject> i_redDotsLoaders = new List<GameObject>();
        List<int> i_indexesToRemove = new List<int>();
        float i_anticipationProgression = attackValues.maxAnticipationTime;
        float i_finalRedDotLength = aimingRedDotTransform.localScale.z;
        Vector3 loadingRedDotGrowth = new Vector3(0.01f, 0.01f, 0);
        float i_timeBetweenLoaderSpawnings = 0.3f;
        float i_loaderSpawnCooldown = 0;


        while (i_anticipationProgression > 0)
        {
            if (i_anticipationProgression > attackValues.maxAnticipationTime * 0.6f)  // From start to 40% of completion
            {
                //Spawn redDot loader periodically
                if (i_loaderSpawnCooldown <= 0)
                {
                    GameObject i_newRedDot = Instantiate(redDotPrefab, aimingRedDotTransform);
                    i_newRedDot.transform.LookAt(aimingRedDotTransform.transform.position + Vector3.up);
                    i_newRedDot.transform.localScale = new Vector3(i_newRedDot.transform.localScale.x, i_newRedDot.transform.localScale.y, i_finalRedDotLength);
                    i_redDotsLoaders.Add(i_newRedDot);
                    i_loaderSpawnCooldown = i_timeBetweenLoaderSpawnings;
                }
                else
                {
                    i_loaderSpawnCooldown -= Time.deltaTime;
                }
            }

            if (i_anticipationProgression > attackValues.maxAnticipationTime * 0.2f) // From start to 80% of completion
            {
                if (i_redDotsLoaders.Count > 0)
                {
                    for (int i = 0; i < i_redDotsLoaders.Count-1; i++)
                    {
                        Quaternion wantedRotation = Quaternion.LookRotation(aimingRedDotTransform.position + aimingRedDotTransform.forward);
                        wantedRotation.eulerAngles = new Vector3(0, wantedRotation.eulerAngles.y, 0);
                        i_redDotsLoaders[i].transform.rotation = Quaternion.Lerp(i_redDotsLoaders[i].transform.rotation, wantedRotation, i_anticipationProgression);

                        if (Vector3.Dot(i_redDotsLoaders[i].transform.forward, aimingRedDotTransform.forward) <= 1 && Vector3.Dot(i_redDotsLoaders[i].transform.forward, aimingRedDotTransform.forward) > 0.99)
                        {
                            i_indexesToRemove.Add(i);
                            aimingRedDotTransform.localScale += loadingRedDotGrowth;
                        }
                    }
                }
            }
            else
            {
                // Reduce redDot size, to "go back" to turret before firing
                aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Mathf.Lerp(i_finalRedDotLength, 0, i_anticipationProgression / attackValues.maxAnticipationTime * 0.2f));
            }

            // clean the useless redDotsLoaders
            foreach (var index in i_indexesToRemove)
            {
                Destroy(i_redDotsLoaders[index]);
                i_redDotsLoaders.RemoveAt(index);
            }
            i_indexesToRemove.Clear();


            i_anticipationProgression -= Time.deltaTime;
            yield return null;
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
        //if (laserShootingCoroutine != null)
        //{
        //    StopCoroutine(laserShootingCoroutine);
        //    laserShootingCoroutine = null;
        //}
        //if (spawnedBullet != null)
        //{
        //    Destroy(spawnedBullet);
        //}
    }
}
