using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTurretSniperBehaviour : TurretBehaviour
{
    [Header("Boss Sniper")]
    public bool OnBoss = true;
    public bool Activated = true;
    public float HeightDropped;

    public GameObject newParentWhenDropped;
    public Transform PlayerTargeted;

    public Vector2 minMaxFollowingAimingCubeScale;
    public Transform aimingAtPlayerFXTransform;
    public Renderer aimingAtPlayerFXRenderer;
    public Vector3 aimingAtPlayerFXScaleOnWall;
    public Vector3 aimingAtPlayerFXScaleOnPlayer;
    public float endAimingFXScaleMultiplier;
    public float startAimingFXCircleThickness;
    public MeshRenderer meshRenderer;
    public Material TurretActivated;
    public Material TurretDesactivated;

    public override void Shoot()
    {
        if (Activated)
        {
            Vector3 i_spawnPosition;
            i_spawnPosition = bulletSpawn.position;
            spawnedBullet = Instantiate(bulletPrefab, i_spawnPosition, Quaternion.LookRotation(focusedPawnController.transform.position + Vector3.up));
            spawnedBullet.transform.LookAt(focusedPawnController.transform.position + Vector3.up * 3);
            spawnedBullet.GetComponent<TurretSniperBullet>().target = focusedPawnController.transform;
            spawnedBullet.GetComponent<TurretSniperBullet>().spawnParent = transform;

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

    public void ActivateTurret()
    {
        Activated = true;
        meshRenderer.material = TurretActivated;
        // focusedPawnController.transform = PlayerTargeted;
    }

    public void DesactivateTurret()
    {
        Activated = false;
        meshRenderer.material = TurretDesactivated;
    }


    public void BossDropTurret()
    {
        transform.SetParent(newParentWhenDropped.transform);
        transform.position = new Vector3(transform.position.x, HeightDropped, transform.position.z);
        Activated = true;
        invincibilityTime = 0f;
        OnBoss = false;
    }


    protected override void Update()
    {
        base.Update();
        if (OnBoss)
        {
            // focusedPawnController.transform = PlayerTargeted;
            invincibilityTime = 0.1f;
        }
        if (Activated)
        {
            UpdateAimingCubeState();
            aimingAtPlayerFXTransform.gameObject.SetActive(true);
            aimingRedDotTransform.gameObject.SetActive(true);
        }
        else
        {
            aimingAtPlayerFXTransform.gameObject.SetActive(false);
            aimingRedDotTransform.gameObject.SetActive(false);
        }
    }


    protected override void RotateTowardsPlayerAndHisForward(float _rotationSpeedModRatio = 0)
    {
        wantedRotation = Quaternion.LookRotation(focusedPawnController.transform.position + Vector3.up * 1 + focusedPawnController.transform.forward * focusedPawnController.transform.GetComponent<Rigidbody>().velocity.magnitude * 0.1f - modelPivot.position);
        wantedRotation.eulerAngles = new Vector3(wantedRotation.eulerAngles.x, wantedRotation.eulerAngles.y, wantedRotation.eulerAngles.x);

        modelPivot.rotation = Quaternion.Lerp(modelPivot.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed));
    }


    //unused
    protected override void RotateTowardsPlayerPosition(float _rotationSpeedModRatio = 0)
    {
        wantedRotation = Quaternion.LookRotation(focusedPawnController.transform.position * 1 - modelPivot.position);
        wantedRotation.eulerAngles = new Vector3(wantedRotation.eulerAngles.x, wantedRotation.eulerAngles.y, wantedRotation.eulerAngles.x);
        modelPivot.rotation = Quaternion.Lerp(modelPivot.rotation, wantedRotation, Time.deltaTime * Mathf.Abs(maxRotationSpeed));
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
        bool i_aimAtPlayer;

        if (Physics.Raycast(modelPivot.position, modelPivot.forward, Vector3.Distance(modelPivot.position, focusedPawnController.transform.position), layersToCheckToScale))
        {
            i_aimAtPlayer = false;
        }
        else
        {
            i_aimAtPlayer = true;
        }
        //Adapt aimCube Scale and Position
        RaycastHit hit;
        if (Physics.Raycast(modelPivot.position, modelPivot.forward, out hit, 50, layersToCheckToScale))
        {
            aimingRedDotTransform.localScale = new Vector3(aimingRedDotTransform.localScale.x, aimingRedDotTransform.localScale.y, Vector3.Distance(modelPivot.position, hit.point));
            aimingRedDotTransform.position = modelPivot.position + (aimingRedDotTransform.localScale.z / 2 * modelPivot.forward);
        }

        //Adapt PlayerFXRenderer

        switch (attackState)
        {
            //-------------------------------------------------------
            case TurretAttackState.Anticipation:
                //ADAPT FXs
                if (i_aimAtPlayer)
                {
                    aimingAtPlayerFXTransform.position = focusedPawnController.transform.position;
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
                if (focusedPawnController.transform != null)
                {
                    RotateTowardsPlayerAndHisForward();
                    // rotating an object here works, so the method must be malfunctionning or the mainbody rotation is touched somewhere else
                }

                //TRANSITION TO OTHER STATE
                anticipationTime -= Time.deltaTime;
                aimingAtPlayerFXRenderer.material.SetFloat("_CircleThickness", Mathf.Lerp(startAimingFXCircleThickness, 1, 1 - (anticipationTime / maxAnticipationTime)));
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
                    aimingAtPlayerFXTransform.position = focusedPawnController.transform.position;
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
                if (focusedPawnController.transform != null)
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

                if (aimingRedDotState != AimingRedDotState.NotVisible)
                {
                    ChangeAimingRedDotState(AimingRedDotState.NotVisible);
                }

                if (focusedPawnController.transform != null)
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
                if (i_randomFloat > 0.5f)
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
