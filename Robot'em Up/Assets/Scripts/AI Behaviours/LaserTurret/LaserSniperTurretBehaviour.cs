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
    private IEnumerator redDotLoadingCoroutine;
    private GameObject FXChargingParticlesInstance;
    private GameObject FXChargingMainLaserInstance;

    public GameObject redDotPrefab;


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
            i_instance.target = focusedPlayer.transform;
            i_instance.spawnParent = transform;
            shootingLaserTimeProgression = shootingLaserMaxTime;
            timeToTriggerLaserReduction = shootingLaserMaxTime - (whenToTriggerLaserReduction * shootingLaserMaxTime);

        }

        float normalLaserWidth = i_instance.laserWidth;

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
                //i_laserRenderer.material.color = new Color(i_laserRenderer.material.color.r, i_laserRenderer.material.color.g, i_laserRenderer.material.color.b, i_laserRenderer.material.color.a * reducingLaserFactor);

                i_instance.laserWidth = normalLaserWidth * reducingLaserFactor;
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
                AbortAttack();
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
                AbortAttack();
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
                ChangeAimingRedDotState(AimingRedDotState.Following);
                //VARIABLES GAMEPLAY------------------
                attackState = TurretAttackState.Anticipation;
                animator.SetTrigger("AnticipationTrigger");
                anticipationTime = maxAnticipationTime;
                restTime = maxRestTime + Random.Range(-randomRangeRestTime, randomRangeRestTime);
                //VARIABLES FXs--------------------------------------
                FXChargingParticlesInstance = Instantiate(FXChargingParticlesPrefab, bulletSpawn.position, Quaternion.identity, modelPivot);
                FXChargingMainLaserInstance = Instantiate(FXChargingMainLaserPrefab, bulletSpawn.position, Quaternion.identity, modelPivot);   // has a 0.1sec start delay to make it look like the charging particles created it
                aimingAtPlayerFXRenderer.material.SetFloat("_AddToCompleteCircle", 1);
                redDotLoadingCoroutine = null;
                laserShootingCoroutine = null;
                break;
            case TurretState.Idle:
                timeBetweenCheck = 0;
                break;
        }
    }

    public override void AttackingUpdateState()
    {
        //Adapt aimCube Scale and Position
        RaycastHit hit;
        if (Physics.Raycast(modelPivot.position, modelPivot.forward, out hit, laserMaxLength, layersToCheckToScale))
        {
            aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Vector3.Distance(transform.position, hit.point));
            //aimingRedDotTransform.position = transform.position + transform.up * .5f + (aimingRedDotTransform.localScale.z / 2 * transform.forward);
        }
        else
        {
            aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Vector3.Distance(transform.position, transform.position + transform.forward * 50));
        }

        switch (attackState)
        {
            //-------------------------------------------------------
            case TurretAttackState.Anticipation:
                aimingAtPlayerFXTransform.gameObject.SetActive(true) ;
                aimingAtPlayerFXTransform.position = new Vector3(hit.point.x, aimingRedDotTransform.position.y, hit.point.z) + hit.normal * 0.2f;
                aimingAtPlayerFXTransform.rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(180, 0, 0);
                aimingAtPlayerFXTransform.localScale = aimingAtPlayerFXScaleOnWall;

                FXChargingMainLaserInstance.transform.localScale = Vector3.one * Mathf.Clamp(maxAnticipationTime / anticipationTime + 0.01f, 1, 6);

                if (redDotLoadingCoroutine == null)
                {
                    //redDotLoadingCoroutine = UpdateAnticipationRedDot_C(); 
                    //StartCoroutine(redDotLoadingCoroutine);
                }


                //ROTATE TOWARDS PLAYER
                if (focusedPlayer != null)
                {
                    RotateTowardsPlayerPosition();
                }

                //TRANSITION TO OTHER STATE
                anticipationTime -= Time.deltaTime;

                aimingAtPlayerFXTransform.localScale *= Mathf.Lerp(1, endAimingFXScaleMultiplier, 1 - (anticipationTime / maxAnticipationTime));

                if (anticipationTime < maxAnticipationTime * 0.2)
                {
                    ParticleSystem[] i_systems = FXChargingParticlesInstance.GetComponentsInChildren<ParticleSystem>();
                    foreach (var system in i_systems)
                    {
                        system.Stop();
                    }
                } 

                if (anticipationTime <= 0)
                {
                    attackState = TurretAttackState.Attack;
                    animator.SetTrigger("StartLaserTrigger");
                    attackDuration = shootingLaserMaxTime;
                    Shoot();
                }
                break;
            //-------------------------------------------------------
            case TurretAttackState.Attack:
                //ADAPT FXs----------------------------------
                aimingAtPlayerFXTransform.gameObject.SetActive(false);

                Destroy(FXChargingParticlesInstance);
                Destroy(FXChargingMainLaserInstance);

                ChangeAimingRedDotState(AimingRedDotState.NotVisible);

                //ROTATE TOWARDS PLAYER-------------------------------------
                if (focusedPlayer != null)
                {
                    RotateTowardsPlayerPosition(rotationSpeedReductionRatio);   // rotate slower toward player
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

                break;
        }
    }

    void AbortAttack()
    {
        if (FXChargingParticlesInstance) { Destroy(FXChargingParticlesInstance); }
        if (FXChargingMainLaserInstance) { Destroy(FXChargingMainLaserInstance); }
        if (aimingRedDotState != AimingRedDotState.NotVisible)
        {
            ChangeAimingRedDotState(AimingRedDotState.NotVisible);
        }

    }

    IEnumerator UpdateAnticipationRedDot_C() // WIP, NOT FINISHED, MUST BE FINISHED
    {
        List<GameObject> i_redDotsLoaders = new List<GameObject>();
        List<int> i_indexesToRemove = new List<int>();
        float i_anticipationProgression = maxAnticipationTime;
        float i_finalRedDotLength = aimingRedDotTransform.localScale.z;
        Vector3 loadingRedDotGrowth = new Vector3(0.01f, 0.01f, 0);
        float i_timeBetweenLoaderSpawnings = 0.3f;
        float i_loaderSpawnCooldown = 0;


        while (i_anticipationProgression > 0)
        {
            Debug.Log("chiotte");
            if (i_anticipationProgression > maxAnticipationTime * 0.6f)  // From start to 40% of completion
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

            if (i_anticipationProgression > maxAnticipationTime * 0.2f) // From start to 80% of completion
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
                            Debug.Log("growing the main red Dot");
                            i_indexesToRemove.Add(i);
                            aimingRedDotTransform.localScale += loadingRedDotGrowth;
                        }
                    }
                }
            }
            else
            {
                // Reduce redDot size, to "go back" to turret before firing
                aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Mathf.Lerp(i_finalRedDotLength, 0, i_anticipationProgression / maxAnticipationTime * 0.2f));
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
        if (laserShootingCoroutine != null)
        {
            StopCoroutine(laserShootingCoroutine);
            laserShootingCoroutine = null;
        }
        if (spawnedBullet != null)
        {
            Destroy(spawnedBullet);
        }
    }
}
