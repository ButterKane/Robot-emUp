﻿using MyBox;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : PawnController, IHitable
{
    public bool BossActivated = false;
    [Space(2)]
    [Separator("References")]
    public HealthBar healthBar1;
    public HealthBar healthBar2;
    public GameObject healthBarPrefab;
    [SerializeField] protected Transform playerOneTransform;
    protected PawnController playerOnePawnController;
    [SerializeField] protected Transform playerTwoTransform;
    protected PawnController playerTwoPawnController;
    protected bool playerOneInRange;
    protected bool playerTwoInRange;

    [Separator("Boss Variables")]
    public float meleeRange = 20;
    public float missileRange = 40;
    protected float distanceWithPlayerOne;
    protected float distanceWithPlayerTwo;
    public ArenaDoor endDoor;

    public Renderer[] renderers;
    public Color normalColor = Color.blue;
    public Color attackingColor = Color.red;
    [SerializeField] protected bool lockable; public bool lockable_access { get { return lockable; } set { lockable = value; } }
    [SerializeField] protected float lockHitboxSize; public float lockHitboxSize_access { get { return lockHitboxSize; } set { lockHitboxSize = value; } }


    public enum BossState
    {
        Idle,
        Moving,
        Stagger,
        PunchAttack,
        RangeAttack,
        Shield,
        HammerPunchAttack,
        GroundAttack,
        Laser,
        ChangingPhase
    }

    public List<BossState> ListPatternPhase1;
    public List<BossState> ListPatternPhase2;
    public float waitingBeforeNextState;
    public bool isPhase1;

    public BossState bossState = BossState.Idle;
    public int Health1Bar_Value_Max = 100;
    public int Health2Bar_Value_Max = 50;
    [ReadOnly] public int Health1Bar_CurrentValue = 0;
    [ReadOnly] public int Health2Bar_CurrentValue = 0;


    [Space(2)]
    [Header("Punch Attack")]
    public int PunchAttack_DamageInflicted = 20;
    public float PunchAttack_Anticipation = 0.5f;
    public float PunchAttack_AttackingTime = 0.3f;
    public float PunchAttack_RecoverTime = 0.8f;
    public bool PunchAttack_Attacking= false;
    public GameObject PunchAttack_attackHitBoxPrefab;
    public GameObject PunchAttack_attackHitBoxInstance;
    public Vector3 PunchAttack_hitBoxOffset;

    [Header("Range Attack")]
    public float RangeAttack_Anticipation = 0.5f;
    public float RangeAttack_AttackDuration = 1.2f;
    public float RangeAttack_RecoverTime = 0.8f;
    public float RangeAttack_Damage = 0.4f;
    public Vector3 RangeAttack_SpawnPosition;
    public GameObject RangeAttack_BulletPrefab;
    public GameObject RangeAttack_spawnedBullet1;
    public GameObject RangeAttack_spawnedBullet2;
    public bool RangeAttack_Attacking = false;
    public Transform aimingCube1_Transform;
    public Renderer aimingCube1_Renderer;
    public Transform aimingCube2_Transform;
    public Renderer aimingCube2_Renderer;
    public AimingRedDotState aimingCubeState;
    public Color lockingAimingColor;
    public float lockingAimingColorIntensity;
    public Color followingAimingColor;
    public float followingAimingColorIntensity;
    public float maxRotationSpeed = 1;


    [Header("Shield Invocation")]
    public GameObject BossShield;
    public bool ShieldIsActivated = false;
    public float ShieldInvocation_Rate = 0.03f;
    public float ShieldInvaction_MinHeath = 0.75f;
    public float ShieldInvaction_TimeInvocation = 0.75f;
    public float ShieldInvocation_ShieldTimeLasting = 15f;
    public float ShieldInvocation_CurrentTimeLasting = 0;


    [Header("ChangingPhase")]
    public float Stagger_TimeLasting = 4f;
    public GameObject Turret_Prefab;


    [Header("GroundAttack")]
    public List<PuzzleEletricPlate> ListEletricPlates;
    public bool GroundAttack_Attacking = false;
    public float GroundAttackInvocation_Rate = 0.03f;
    public float GroundAttack_AnticipationTime = 1f;
    public float GroundAttack_AttackTime = 1f;


    [Header("HammerAttack")]
    public bool HammerAttack_Attacking;
    public int NumberHammerAttackMax = 6;
    public int NumberHammerLeft = 0;
    public float HammerAttack_Anticipation = 1f;
    public float HammerAttack_AttackingTime = 0.7f;
    public float HammerAttack_RecoverTime = 1.0f;
    public GameObject HammerAttack_attackHitBoxPrefab;
    public GameObject HammerAttack_attackHitBoxInstance;
    public GameObject ObstacleHammerAttackPrefab;


    [Header("LaserAttack")]
    public int SequenceLaserAttack;
    public float LaserAttack_Anticipation = 1.0f;
    public float LaserAttack_AttackingTime = 5.0f;
    public float laserAttackInvocation_Rate = 90;
    public float laserSpeed = 130;
    public GameObject LaserAttack_LaserObject;
    public Vector3 LaserAttack_ArenaCenter;


    public void StartBoss()
    {
        Health1Bar_CurrentValue = Health1Bar_Value_Max;
        Health2Bar_CurrentValue = Health2Bar_Value_Max;
        healthBar1 = Instantiate(healthBarPrefab, GameManager.mainCanvas.transform).GetComponent<HealthBar>();
        healthBar1.customHealthBar = true;
        healthBar1.target = this;

        healthBar2 = Instantiate(healthBarPrefab, GameManager.mainCanvas.transform).GetComponent<HealthBar>();
        healthBar2.customHealthBar = true;
        healthBar2.customDeltaPosition = -1;
        healthBar2.target = this;

        healthBar1.ToggleHealthBar(true);
        healthBar2.ToggleHealthBar(true);
        isPhase1 = true;

        playerOneTransform = GameManager.playerOne.transform;
        playerTwoTransform = GameManager.playerTwo.transform;
        playerOnePawnController = playerOneTransform.GetComponent<PlayerController>();
        playerTwoPawnController = playerTwoTransform.GetComponent<PlayerController>();

        NumberHammerLeft = NumberHammerAttackMax;
        BossActivated = true;
    }


    public void Update()
    {
        if (BossActivated)
        {
            healthBar1.customValueToCheck = (float)Health1Bar_CurrentValue / Health1Bar_Value_Max;
            healthBar2.customValueToCheck = (float)Health2Bar_CurrentValue / Health2Bar_Value_Max;
            UpdateDistancesToPlayers();
            UpdateAnimatorBlendTree();
            waitingBeforeNextState -= Time.deltaTime;
            ShieldInvocation_CurrentTimeLasting -= Time.deltaTime;

            if (ShieldInvocation_CurrentTimeLasting < 0 && ShieldIsActivated)
            {
                BossShield.SetActive(false);
                ShieldIsActivated = false;
            }

            if (waitingBeforeNextState > 0)
            {
                switch (bossState)
                {
                    case BossState.Idle:
                        break;
                    case BossState.Moving:
                        break;
                    case BossState.Stagger:
                        break;
                    case BossState.PunchAttack:
                        if (waitingBeforeNextState <  PunchAttack_AttackingTime + PunchAttack_RecoverTime && PunchAttack_Attacking == false)
                        {
                            animator.SetTrigger("PunchAttackTrigger");
                            PunchAttack_Attacking = true;

                        }
                        if (waitingBeforeNextState < PunchAttack_RecoverTime && PunchAttack_Attacking == true)
                        {
                            foreach (var renderer in renderers)
                            {
                                renderer.material.SetColor("_Color", Color.Lerp(normalColor, attackingColor, PunchAttack_RecoverTime));
                            }
                            animator.SetTrigger("EndOfPunchAttackTrigger");
                            PunchAttack_Attacking = false;
                        }

                        if (waitingBeforeNextState < PunchAttack_RecoverTime / 3)
                        {
                            navMeshAgent.enabled = true;
                            navMeshAgent.SetDestination(GetClosestAndAvailablePlayer().position);

                        }

                        break;
                    case BossState.RangeAttack:

                        RotateTowardsPlayerAndHisForward(playerOneTransform, aimingCube1_Transform);
                        RotateTowardsPlayerAndHisForward(playerTwoTransform, aimingCube2_Transform);
                        if (waitingBeforeNextState > RangeAttack_AttackDuration + RangeAttack_RecoverTime)
                        {
                            ChangeAimingCubeState(AimingRedDotState.Following);
                        }
                        if (waitingBeforeNextState < RangeAttack_AttackDuration + RangeAttack_RecoverTime && RangeAttack_Attacking == false)
                        {
                            animator.SetTrigger("RangeAttackTrigger");
                            RangeAttack_Attacking = true;
                        }

                        if (waitingBeforeNextState < RangeAttack_RecoverTime / 3)
                        {
                            RangeAttack_Attacking = false;
                            navMeshAgent.enabled = true;
                            navMeshAgent.SetDestination(GetClosestAndAvailablePlayer().position);

                        }
                        break;
                    case BossState.Shield:
                        break;
                    case BossState.HammerPunchAttack:

                        if (waitingBeforeNextState < HammerAttack_AttackingTime + HammerAttack_RecoverTime && HammerAttack_Attacking == false)
                        {
                            animator.SetTrigger("HammerAttackTrigger");
                            HammerAttack_Attacking = true;

                        }
                        if (waitingBeforeNextState < HammerAttack_AttackingTime && HammerAttack_Attacking == true)
                        {
                            foreach (var renderer in renderers)
                            {
                                renderer.material.SetColor("_Color", Color.Lerp(normalColor, attackingColor, HammerAttack_RecoverTime));
                            }
                            animator.SetTrigger("HammerEndOfAttackTrigger");
                            HammerAttack_Attacking = false;
                        }

                        if (waitingBeforeNextState < PunchAttack_RecoverTime / 3)
                        {
                            navMeshAgent.enabled = true;
                            navMeshAgent.SetDestination(GetClosestAndAvailablePlayer().position);

                        }


                        break;
                    case BossState.GroundAttack:

                        if (waitingBeforeNextState < GroundAttack_AttackTime && GroundAttack_Attacking == false)
                        {
                            animator.SetTrigger("GroundAttackAnticipationTrigger");
                            GroundAttack_Attacking = true;
                            ActivateEletricPlate();

                        }
                        if (waitingBeforeNextState < GroundAttack_AttackTime / 7 && GroundAttack_Attacking == true)
                        {
                            animator.SetTrigger("GroundAttackEndTrigger");

                        }
                    

                        break;
                    case BossState.Laser:
                        if (SequenceLaserAttack == 0)
                        {
                            navMeshAgent.enabled = true;
                            navMeshAgent.SetDestination(LaserAttack_ArenaCenter);
                            if (Vector3.Distance(transform.position, LaserAttack_ArenaCenter) < 5)
                            {
                                SequenceLaserAttack = 1;
                            }
                        }
                        if (SequenceLaserAttack == 1)
                        {
                            waitingBeforeNextState = LaserAttack_Anticipation + LaserAttack_AttackingTime;
                            navMeshAgent.enabled = false;
                            SequenceLaserAttack = 2;
                            animator.SetTrigger("LaserAnticipation");
                        }
                        if (waitingBeforeNextState < LaserAttack_AttackingTime && SequenceLaserAttack == 2)
                        {
                            animator.SetTrigger("LaserAttackAttacking");
                            LaserAttack_LaserObject.SetActive(true);
                            LaserAttack_LaserObject.GetComponent<ParticleSystem>().Play();
                            SequenceLaserAttack = 3;

                        }
                        if (SequenceLaserAttack == 3)
                        {
                            LaserAttack_LaserObject.transform.rotation = Quaternion.Euler(LaserAttack_LaserObject.transform.rotation.eulerAngles.x, LaserAttack_LaserObject.transform.rotation.eulerAngles.y + laserSpeed * Time.deltaTime, LaserAttack_LaserObject.transform.rotation.eulerAngles.z);
                        }
                        if (waitingBeforeNextState < LaserAttack_AttackingTime / 8 && SequenceLaserAttack == 3)
                        {
                            animator.SetTrigger("LaserAttackEndAttacking");
                            SequenceLaserAttack = 4;
                            LaserAttack_LaserObject.SetActive(false);

                        }
                        break;
                    case BossState.ChangingPhase:
                        if (waitingBeforeNextState < Stagger_TimeLasting / 4 && isPhase1)
                        {
                            animator.SetTrigger("EndingChangingPhaseTrigger");
                            isPhase1 = false;
                            Instantiate(Turret_Prefab, transform.position + transform.forward * 2, Quaternion.identity);
                            Instantiate(Turret_Prefab, transform.position + transform.forward * -2, Quaternion.identity);
                        }
                        break;
                    default:
                        break;
                }
            }


            if (waitingBeforeNextState < 0)
            {

                if (bossState == BossState.GroundAttack)
                {
                    DesactivateEletricPlate();
                    foreach (var item in ListEletricPlates)
                    {
                        item.gameObject.SetActive(false);
                    }
                }

                ChangeState();
            }

        }
    }




    public void ChangeState()
    {

        if (isPhase1)
        {
            if (Health1Bar_CurrentValue <= 0)
            {
                bossState = BossState.ChangingPhase;
                animator.SetTrigger("ChangingPhaseTrigger");
                waitingBeforeNextState = Stagger_TimeLasting;

            }
            else
            {
                float temproll = Random.Range(0, 100);
                bool WantToInvokeShield = false;
                if (temproll > ShieldInvocation_Rate)
                {
                    if (ShieldIsActivated == false) //Health1Bar_CurrentValue / Health1Bar_Value_Max < ShieldInvaction_MinHeath &&
                    {
                        WantToInvokeShield = true;
                    }
                }

                // If players are too far -> range attack
                if (distanceWithPlayerTwo > missileRange && distanceWithPlayerOne > missileRange)
                {
                    navMeshAgent.enabled = false;
                    if (WantToInvokeShield)
                    {
                        InvokingShieldAttack(); //Shield Invocation is prioritary to firing missile
                    }
                    else
                    {
                        bossState = BossState.RangeAttack;
                        waitingBeforeNextState = RangeAttack_Anticipation + RangeAttack_AttackDuration + RangeAttack_RecoverTime;
                        ChangeAimingCubeState(AimingRedDotState.Following);

                    }

                }
                else
                {
                    //If player are closer but not enough -> walk towards them if alive
                    if (distanceWithPlayerOne < distanceWithPlayerTwo && distanceWithPlayerOne > meleeRange && playerOnePawnController.currentHealth > 0)
                    {
                        if (WantToInvokeShield)
                        {
                            InvokingShieldAttack(); //Shield Invocation is prioritary to moving
                        }
                        else
                        {
                            bossState = BossState.Moving;
                            waitingBeforeNextState = 1 + Random.Range(-0.5f, 0.5f);
                            navMeshAgent.enabled = true;
                            navMeshAgent.SetDestination(playerOneTransform.position);
                        }
                    }


                    //If player are closer but not enough -> walk towards them if alive
                    if (distanceWithPlayerOne > distanceWithPlayerTwo && distanceWithPlayerTwo > meleeRange && playerTwoPawnController.currentHealth > 0)
                    {
                        if (WantToInvokeShield)
                        {
                            InvokingShieldAttack(); //Shield Invocation is prioritary to moving
                        }
                        else
                        {
                            bossState = BossState.Moving;
                            waitingBeforeNextState = 1 + Random.Range(-0.5f, 0.5f);
                            navMeshAgent.enabled = true;
                            navMeshAgent.SetDestination(playerTwoTransform.position);
                        }
                    }


                    if (true)
                    {

                    }

                    //If a player is very close -> punch attack
                    if ((distanceWithPlayerTwo < meleeRange && playerTwoPawnController.currentHealth > 0) | (distanceWithPlayerOne < meleeRange && playerOnePawnController.currentHealth > 0))
                    {
                        bossState = BossState.PunchAttack;
                        waitingBeforeNextState = PunchAttack_Anticipation + PunchAttack_AttackingTime + PunchAttack_RecoverTime;
                        navMeshAgent.enabled = false;
                        animator.SetTrigger("PunchAttackAnticipationTrigger");

                        foreach (var renderer in renderers)
                        {
                            renderer.material.SetColor("_Color", Color.Lerp(attackingColor, normalColor, PunchAttack_Anticipation));
                        }
                    }
                }


            }

        }
        else
        {

            //PHASE 2


            float temproll = Random.Range(0, 100);
            bool GroundAttacking = false;
            if (temproll < GroundAttackInvocation_Rate)
            {
                GroundAttacking = true;
            }
            temproll = Random.Range(0, 100);
            bool LaserAttacking = false;
            if (temproll < laserAttackInvocation_Rate)
            {
                LaserAttacking = true;
            }

            if (GroundAttacking)
            {
                bossState = BossState.GroundAttack;
                waitingBeforeNextState = GroundAttack_AnticipationTime + GroundAttack_AttackTime;
                navMeshAgent.enabled = false;
                foreach (var item in ListEletricPlates)
                {
                    item.gameObject.SetActive(true);
                }
                GroundAttack_Attacking = false;
                animator.SetTrigger("GroundAttackAnticipationTrigger");

            }
            else if(LaserAttacking)
            {
                bossState = BossState.Laser;
                waitingBeforeNextState = 10f;
                SequenceLaserAttack = 0;

            }
            else
            {

                //If player are closer but not enough -> walk towards them if alive
                if (distanceWithPlayerOne < distanceWithPlayerTwo && distanceWithPlayerOne > meleeRange && playerOnePawnController.currentHealth > 0)
                {
                    bossState = BossState.Moving;
                    waitingBeforeNextState = 1 + Random.Range(-0.5f, 0.5f);
                    navMeshAgent.enabled = true;
                    navMeshAgent.SetDestination(playerOneTransform.position);
                }


                //If player are closer but not enough -> walk towards them if alive
                if (distanceWithPlayerOne > distanceWithPlayerTwo && distanceWithPlayerTwo > meleeRange && playerTwoPawnController.currentHealth > 0)
                {
                    bossState = BossState.Moving;
                    waitingBeforeNextState = 1 + Random.Range(-0.5f, 0.5f);
                    navMeshAgent.enabled = true;
                    navMeshAgent.SetDestination(playerTwoTransform.position);
                }



                //If a player is very close -> hammer attack
                if ((distanceWithPlayerTwo < meleeRange && playerTwoPawnController.currentHealth > 0) | (distanceWithPlayerOne < meleeRange && playerOnePawnController.currentHealth > 0))
                {

                    if (NumberHammerLeft > 0)
                    {
                        NumberHammerLeft--;
                        bossState = BossState.HammerPunchAttack;
                        waitingBeforeNextState = HammerAttack_Anticipation + HammerAttack_AttackingTime + HammerAttack_RecoverTime;
                        navMeshAgent.enabled = false;
                        animator.SetTrigger("HammerAttackAnticipationTrigger");


                    }
                    else
                    {
                        bossState = BossState.PunchAttack;
                        waitingBeforeNextState = PunchAttack_Anticipation + PunchAttack_AttackingTime + PunchAttack_RecoverTime;
                        navMeshAgent.enabled = false;
                        animator.SetTrigger("PunchAttackAnticipationTrigger");

                        foreach (var renderer in renderers)
                        {
                            renderer.material.SetColor("_Color", Color.Lerp(attackingColor, normalColor, PunchAttack_Anticipation));
                        }
                    }
                }
            }

        }
    }

    public void InvokingShieldAttack()
    {
        Debug.Log("InvokingShieldAttack");
        animator.SetTrigger("InvokingShieldTrigger");
        bossState = BossState.Shield;
        ShieldIsActivated = true;
        waitingBeforeNextState = ShieldInvaction_TimeInvocation;
        ShieldInvocation_CurrentTimeLasting = ShieldInvocation_ShieldTimeLasting;
    }

    public void ActivateAttackHitBox()
    {
        if (bossState == BossState.PunchAttack)
        {
            FeedbackManager.SendFeedback("event.BossPunchAttack", this);

        }
        PunchAttack_attackHitBoxInstance = Instantiate(PunchAttack_attackHitBoxPrefab, transform.position + PunchAttack_hitBoxOffset.x * transform.right + PunchAttack_hitBoxOffset.y * transform.up + PunchAttack_hitBoxOffset.z * transform.forward, transform.rotation, transform);
    }

    public void ActivateHammerAttackHitBox()
    {
        if (bossState == BossState.HammerPunchAttack)
        {
            FeedbackManager.SendFeedback("event.BossPunchAttack", this);

        }
        HammerAttack_attackHitBoxInstance = Instantiate(HammerAttack_attackHitBoxPrefab, transform.position + PunchAttack_hitBoxOffset.x * transform.right + PunchAttack_hitBoxOffset.y * transform.up + PunchAttack_hitBoxOffset.z * transform.forward, transform.rotation, transform);
    }

    public void InvokeShield()
    {
        Debug.Log("InvokeShield");
        BossShield.SetActive(true);
    }

    public void LaunchMissiles()
    {
        Vector3 i_spawnPosition;
        i_spawnPosition = RangeAttack_SpawnPosition + transform.position;
        RangeAttack_spawnedBullet1 = Instantiate(RangeAttack_BulletPrefab, i_spawnPosition, Quaternion.identity);
        RangeAttack_spawnedBullet1.transform.LookAt(GameManager.playerOne.GetHeadPosition()); //+ new Vector3(0,2,0));
        RangeAttack_spawnedBullet1.GetComponent<TurretBasicBullet>().canHitEnemies = false;
        RangeAttack_spawnedBullet2 = Instantiate(RangeAttack_BulletPrefab, i_spawnPosition + new Vector3(0,0,0.5f), Quaternion.identity);
        RangeAttack_spawnedBullet2.transform.LookAt(GameManager.playerTwo.GetHeadPosition());// + new Vector3(0, 2, 0));
        RangeAttack_spawnedBullet2.GetComponent<TurretBasicBullet>().canHitEnemies = false;
        ChangeAimingCubeState(AimingRedDotState.NotVisible);
    }

    public void DestroyAttackHitBox()
    {
        Debug.Log("DestroyAttackHitBox");
        if (PunchAttack_attackHitBoxInstance != null)
        {
            Debug.Log("PunchAttack_attackHitBoxInstance");
            Destroy(PunchAttack_attackHitBoxInstance);
            PunchAttack_attackHitBoxInstance = null;
        }
        if (HammerAttack_attackHitBoxInstance != null)
        {
            Debug.Log("HammerAttack_attackHitBoxInstance");
            Instantiate(ObstacleHammerAttackPrefab, transform.position + transform.forward * 8 + new Vector3(0, -1.5f, 0), Quaternion.identity);
            Destroy(HammerAttack_attackHitBoxInstance);
            HammerAttack_attackHitBoxInstance = null;
        }
    }

    public override void UpdateAnimatorBlendTree()
    {
        base.UpdateAnimatorBlendTree();
        if (canMove)
        {
            animator.SetFloat("IdleRunBlend", navMeshAgent.velocity.magnitude / navMeshAgent.speed);
        }
    }

    void UpdateDistancesToPlayers()
    {
        distanceWithPlayerOne = Vector3.Distance(transform.position, playerOneTransform.position);
        distanceWithPlayerTwo = Vector3.Distance(transform.position, playerTwoTransform.position);
    }

    Transform GetClosestAndAvailablePlayer()
    {
        if ((distanceWithPlayerOne >= distanceWithPlayerTwo && playerTwoPawnController.IsTargetable())
            || !playerOnePawnController.IsTargetable())
        {
            return playerTwoTransform;
        }
        else if ((distanceWithPlayerTwo >= distanceWithPlayerOne && playerOnePawnController.IsTargetable())
            || !playerTwoPawnController.IsTargetable())
        {
            return playerOneTransform;
        }
        else
        {
            return null;
        }
    }


    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, float _damages, DamageSource _source, Vector3 _bumpModificators = default)
    {
        if (_ball != null)
        {
            InflictDamage(_damages, 1f);
            FeedbackManager.SendFeedback("event.BallHittingEnemy", this, _ball.transform.position, _impactVector, _impactVector);
            EnergyManager.IncreaseEnergy(0.2f);
            if (_source == DamageSource.Dunk)
            {
                InflictDamage(_damages, 1.5f);
            }
        }
        
    }

    public void ActivateEletricPlate()
    {
        foreach (var item in ListEletricPlates)
        {
            item.WhenDesactivate();
        }
    }

    public void DesactivateEletricPlate()
    {
        foreach (var item in ListEletricPlates)
        {
            item.WhenActivate();
        }
    }

    public void InflictDamage(float _damages, float modifier=1)
    {
        if (Health1Bar_CurrentValue > 0)
        {
            Health1Bar_CurrentValue -= (int)(_damages * modifier);

            if (Health1Bar_CurrentValue <= 0)
            {
                Health1Bar_CurrentValue = 0;
            }
        }
        else if (Health2Bar_CurrentValue > 0)
        {

            Health2Bar_CurrentValue -= (int)(_damages * modifier);

            if (Health2Bar_CurrentValue <= 0)
            {
                navMeshAgent.enabled = false;
                bossState = BossState.Stagger;
                waitingBeforeNextState = 99999;
                animator.SetTrigger("DeathTrigger");
                Health2Bar_CurrentValue = 0;
                endDoor.OpenDoor();
            }
        }
        else
        {
            // shouldnotHappen
        }
        
    }

    protected virtual void RotateTowardsPlayerAndHisForward(Transform focusedPlayer, Transform Turret)
    {
        Quaternion wantedRotation = Quaternion.identity;
        wantedRotation = Quaternion.LookRotation(focusedPlayer.position);
        Turret.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed));
    }

    public virtual void ChangeAimingCubeState(AimingRedDotState _newState)
    {
        if (_newState == AimingRedDotState.Following)
        {
            aimingCube1_Transform.gameObject.SetActive(true);
            aimingCube2_Transform.gameObject.SetActive(true);
            aimingCube1_Renderer.material.color = followingAimingColor;
            aimingCube1_Renderer.material.SetColor("_EmissionColor", followingAimingColor * followingAimingColorIntensity);
            aimingCube2_Renderer.material.color = followingAimingColor;
            aimingCube2_Renderer.material.SetColor("_EmissionColor", followingAimingColor * followingAimingColorIntensity);
        }
        else if (_newState == AimingRedDotState.Locking)
        {
            aimingCube1_Transform.gameObject.SetActive(true);
            aimingCube2_Transform.gameObject.SetActive(true);
            aimingCube1_Renderer.material.color = lockingAimingColor;
            aimingCube1_Renderer.material.SetColor("_EmissionColor", lockingAimingColor * lockingAimingColorIntensity);
            aimingCube2_Renderer.material.color = lockingAimingColor;
            aimingCube2_Renderer.material.SetColor("_EmissionColor", lockingAimingColor * lockingAimingColorIntensity);
        }
        else if (_newState == AimingRedDotState.NotVisible)
        {
            aimingCube1_Transform.gameObject.SetActive(false);
            aimingCube2_Transform.gameObject.SetActive(false);
        }
        aimingCubeState = _newState;
    }

}
    

