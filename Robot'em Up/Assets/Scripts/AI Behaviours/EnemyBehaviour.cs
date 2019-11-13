﻿using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 0649


public enum EnemyState
{
    Idle,
    Moving,
    Following,
    Attacking,
    Surrounding,
    Null,
    Count
}

public enum WhatBumps
{
    Pass,
    Dunk,
    Environment,
    Count
}

public class EnemyBehaviour : MonoBehaviour, IHitable
{
    [Separator("References")]
    [SerializeField] private Transform _self;
    public Rigidbody Rb;
    [SerializeField] private ParticleSystem _hitFXPrefab;
    [SerializeField] private ParticleSystem _destroyedFXPrefab;
    [SerializeField] private EnemyAttack _attackScript;
    public Animator Animator;

    [Space(2)]
    [Separator("Auto-assigned References")]
    public Transform Target;
    [SerializeField] private Transform _playerOne;
    [SerializeField] private Transform _playerTwo;

    [Space(2)]
    [Separator("Read-only Variables")]
    public EnemyState State;
    
    [Header("Follow parameters")]
    public bool IsFollowingPlayer;
    public float FollowSpeed = 100f;
    public bool IsAttacking = false;
    public float Speed = 100;

    [Space(2)]
    [Separator("Variables")]
    public AnimationCurve Acceleration;
    public float TimeToReachMaxSpeed = 0.5f;
    public float MaxSpeed = 100f;
    [Header("Health")]
    public int MaxHealth = 100;
    public int Health;
    [Header("Attack parameters")]
    public float AttackDistance = 7f;
    public float PushForce = 300;
    [Header("Change Focus")]
    public float focusChangeDifferencialReference = 2f; // marge to apply when comparing distances to both players
    public float focusChangeWaitTime = 0.5f;
    public float focusChangeSpeed = 2f;
    public AnimationCurve ChangeFocusSpeedCurve;

    [Space(2)]
    [Separator("Surrounding Variables")]
    public float TimeBeforeSurround = 2f;
    [Range(0, 1)]
    public float BezierCurveHeight = 0.5f;
    public float BezierDistanceToHeightRatio = 100f;
    public float DistanceToDestinationTolerance = 0.2f; // the estimated distance where it's considered the enemy is close enough to the surrounding point
    [HideInInspector] public Transform ClosestSurroundPoint;

    // Private variables 
    private float _distanceToOne;
    private float _distanceToTwo;
    private Surrounder _surrounder;

    [Space(2)]
    [Separator("Bump/Stagger Variables")]
    public float moveMultiplicator;
    public float normalMoveMultiplicator = 1;
    public AnimationCurve speedRecoverCurve;
    private bool isBumped;

    private int _hitCount;

    private IEnumerator currentCoroutine;

    public int hitCount
    {
        get
        {
            return _hitCount;
        }
        set
        {
            _hitCount = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _playerOne = GameManager.i.playerOne.transform;
        _playerTwo = GameManager.i.playerTwo.transform;

        Health = MaxHealth;
        IsFollowingPlayer = false;
        State = EnemyState.Idle;
        GameManager.i.enemyManager.enemies.Add(this);

        moveMultiplicator = normalMoveMultiplicator;

        WhatShouldIDo(EnemyState.Idle);
        _self = transform;
    }
    
    void Update()
    {
        Animator.SetFloat("IdleRunBlend", Speed/MaxSpeed);
    }

    public void WhatShouldIDo(EnemyState priorityState = EnemyState.Null)
    {
        if (currentCoroutine != null) { StopCoroutine(currentCoroutine); }
        GetTarget();

        if (priorityState != EnemyState.Null)
        {
            State = priorityState;
        }
        else
        {
            if ((Target.position - _self.position).magnitude < AttackDistance)
            {
                if (GameManager.i.enemyManager.enemyCurrentlyAttacking == null)
                {
                    GameManager.i.enemyManager.enemyCurrentlyAttacking = _self.gameObject;
                    State = EnemyState.Attacking;
                }
                else
                {
                    State = EnemyState.Idle;
                }
            }
            else if (Target != null)
            {
                State = EnemyState.Following;
            }
        }

        Debug.Log("WhatShouldIDo? : " + State);

        // Compute the choice according to the state
        switch (State)
        {
            case EnemyState.Idle:
                currentCoroutine = WaitABit();
                StartCoroutine(currentCoroutine);
                break;

            case EnemyState.Moving:
                // Simple move in an area, not following a player
                break;

            case EnemyState.Following:
                currentCoroutine = FollowPlayer(Target);
                StartCoroutine(currentCoroutine);
                break;

            case EnemyState.Attacking:
                LaunchAttack(Target);
                break;

            case EnemyState.Surrounding:
                LaunchSurrounding();
                break;

            default:
                State = EnemyState.Idle; // Making sure the enemy never "bugs" and ends up with no state
                break;
        }
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PawnController _thrower, int _damages, DamageSource _source)
    {
        Debug.Log("Damage taken " + _source);
        DamageTaken(_damages);
        _ball.Explode(true);
        MomentumManager.IncreaseMomentum(0.1f);

        StopEverythingMethod();

        WhatShouldIDo();
    }

    #region Collisions managing
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (collision.gameObject.GetComponent<PawnController>().isInvincible == false)
            {
                Vector3 newCollisionPoint = new Vector3(collision.GetContact(0).point.x, collision.gameObject.transform.position.y, collision.GetContact(0).point.z); // Make sure the impact is "leveled" and not with a y angle
                collision.gameObject.GetComponent<PawnController>().Push((newCollisionPoint - _self.position).normalized, PushForce, newCollisionPoint);
                collision.gameObject.GetComponent<PawnController>().Damage(10);
            }
            StopAllCoroutines();
            WhatShouldIDo();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.i.ball)
        {
            DamageTaken(GameManager.i.ballDamage);
        }
    }
    #endregion

    #region Launcher Functions
    public void LaunchStaggerSequence()
    {
        StartCoroutine(HinderMovementSpeed());
    }
    
    public void LaunchBumpSequence(float upForce, Vector3 pushForce, float bumpedKoTime)
    {
        isBumped = false;
        //currentCoroutine = BumpSequence(upForce, pushForce, bumpedKoTime);
        StartCoroutine(currentCoroutine);
    }

    public void LaunchHinderMovementSpeed()
    {
        StartCoroutine(HinderMovementSpeed());
    }
    
    public void LaunchSurrounding()
    {
        currentCoroutine = SurroundPlayer(Target.gameObject);
        StartCoroutine(currentCoroutine);
    }

    public void LaunchAttack(Transform target)
    {
        transform.LookAt(SwissArmyKnife.GetFlattedDownPosition(target.position, _self.position));
        StartCoroutine(_attackScript.Attack(target));
        Animator.SetTrigger("PrepareAttack");
        _attackScript.LaunchAttack(target);
    }

    public void LaunchChangeFocusSequence()
    {
        currentCoroutine = ChangeFocusSequence();
        StopEverythingMethod();
        StartCoroutine(currentCoroutine);
    }

    public IEnumerator ChangeFocusSequence()
    {
        Animator.SetBool("ChangingFocus", true);
        _self.LookAt(SwissArmyKnife.GetFlattedDownPosition(Target.position, _self.position));

        // TODO: add a progressive turn


        yield return new WaitForSeconds(focusChangeWaitTime);
        Animator.SetBool("ChangingFocus", false);
        WhatShouldIDo();
    }
    #endregion
    
    #region Coroutines

    public IEnumerator WaitABit()
    {
        moveMultiplicator = 0;
        yield return new WaitForSeconds(0.5f);
        moveMultiplicator = normalMoveMultiplicator;
        WhatShouldIDo();
    }

    public IEnumerator SurroundPlayer(GameObject player)
    {
        float distanceFromStartToNow = 0f;

        Transform surroundingPoint = ClosestSurroundPoint;

        /// Bezier quadratic curve : (1-avancement)^2*p0 + 2(1-avancement)*avancement*p1 + avancement^2*p2
        /// With p0,p1,p2 as Vector3 positions, p0 & p2 as start an end points

        if (surroundingPoint == null) // If there's no point to use to surround, abort
        {
            WhatShouldIDo();
        }
        else
        {
            float distanceToPointRatio = (1 + (_self.position - surroundingPoint.position).magnitude / BezierDistanceToHeightRatio);  // widens the arc of surrounding the farther the surroundingPoint is

            Vector3 p0 = _self.position;

            //Vector3 p2 = script.GetAPositionFromPoint(surroundingPoint);

            Vector3 p2 = SwissArmyKnife.GetFlattedDownPosition(surroundingPoint.position, _self.position);

            float angle = Vector3.SignedAngle(p2 - p0, player.transform.position - p0, Vector3.up);

            int moveSens = angle > 1 ? 1 : -1;

            Vector3 p1 = p0 + (p2 - p0) / 0.5f + Vector3.Cross(p2 - p0, Vector3.up) * moveSens * BezierCurveHeight * distanceToPointRatio;

            float distanceToEnd1 = (p2 - p0).magnitude;
            float distanceToEnd2 = (p2 - p0).magnitude;

            float t = 0f;                    // movement avancement on the curve
            float timeNotMovingMuch = 0f;   // checks if blocked by somethign during the movement

            do
            {
                transform.LookAt(SwissArmyKnife.GetFlattedDownPosition(Target.position, _self.position));
                // Getting base infos
                p0 = _self.position;
                distanceToEnd1 = (p2 - p0).magnitude;   // distance from enemy to target
                p2 = SwissArmyKnife.GetFlattedDownPosition(surroundingPoint.position, _self.position);
                distanceToEnd2 = (p2 - p0).magnitude;   // distance from enemy to target after recalculating target position

                Debug.DrawRay(p2, Vector3.up * 3, Color.green);

                // Getting third point of bezier curve
                p1 = p0 + (p2 - p0) / 0.5f + Vector3.Cross(p2 - p0, Vector3.up) * moveSens * BezierCurveHeight;

                // Calculating position on bezier curve, following start point, end point and avancement
                Vector3 positionOnBezierCurve = (Mathf.Pow((1 - t), 2) * p0) + (2 * (1 - t) * t * p1) + (Mathf.Pow(t, 2) * p2);


                _self.position = positionOnBezierCurve;


                // Adapting the avancement with the distance to travel
                float distanceTraveled = distanceToEnd1 * t;    // traveled distance in units

                if (distanceToEnd1 != distanceToEnd2)
                {
                    float distanceTraveledTransfered = distanceTraveled / distanceToEnd2;   // traveled distance transfered on new distance
                    t = distanceTraveledTransfered;    // sets the current avancement according to new distance
                }

                // Stopping the enemy if it's blocked
                distanceFromStartToNow = (positionOnBezierCurve - p0).magnitude;

                if ((_self.position - p0).magnitude < distanceFromStartToNow)    // Comparing the traveled distance and the supposed distance to see if blocked
                {
                    timeNotMovingMuch += Time.deltaTime;
                }
                else
                {
                    timeNotMovingMuch = 0f;
                }

                //Incrementing the avancement
                if (t < 1)
                {
                    t += Time.deltaTime / (distanceToEnd2 * (MaxSpeed/10));
                }

                yield return null;
            } while ((p0 - p2).magnitude > DistanceToDestinationTolerance && timeNotMovingMuch < 0.5f);

            if (timeNotMovingMuch > 0.5f)
            {
                Debug.Log("stopped because not moving enough");
            }
            yield return null;
            
            WhatShouldIDo();
        }
    }
    
    public IEnumerator HinderMovementSpeed(WhatBumps? cause = default)
    {
        switch (cause)
        {
            case WhatBumps.Pass:
                // Fetch ball datas => speed reduction on pass
                break;
            case WhatBumps.Dunk:
                // Fetch ball datas => speed reduction on dunk
                break;
            case WhatBumps.Environment:
                // Fetch environment datas => speed reduction
                break;
            default:
                moveMultiplicator -= 0.5f;
                Debug.Log("Default case: New speed multiplicator = 0.5");
                break;
        }
        
        float t = moveMultiplicator;
        
        while (moveMultiplicator < normalMoveMultiplicator)
        {
            moveMultiplicator = normalMoveMultiplicator * speedRecoverCurve.Evaluate(t);
            t += Time.deltaTime;
            yield return null;
        }

        moveMultiplicator = normalMoveMultiplicator;
    }
    
    public IEnumerator FollowPlayer(Transform playerToFollow)
    {
        Animator.SetBool("FollowingPlayer", true);
        float followTime = 0;
        float t = 0;
        while ((Target.position - _self.position).magnitude > AttackDistance && followTime <= TimeBeforeSurround)
        {
            Speed = MaxSpeed * moveMultiplicator * Acceleration.Evaluate(t);

            _self.LookAt(SwissArmyKnife.GetFlattedDownPosition(playerToFollow.position, _self.position));

            Rb.velocity = Vector3.zero;
            
            Rb.AddForce(_self.forward * FollowSpeed * Time.deltaTime, ForceMode.VelocityChange);

            followTime += Time.deltaTime;
            Rb.AddForce(_self.forward * Speed * Time.deltaTime, ForceMode.VelocityChange);

            if (Speed < MaxSpeed * moveMultiplicator)
            {
                t += Time.deltaTime/ TimeToReachMaxSpeed;
            }
            else // wait until max speed is reached, then start counting
            {
                followTime += Time.deltaTime;
            }
            yield return null;
        }

        if (followTime >= TimeBeforeSurround)
        {
            if ((Target.gameObject == GameManager.i.playerOne && EnemyManager.i.enemyGroupOne.Contains(this))
                || (Target.gameObject == GameManager.i.playerTwo && EnemyManager.i.enemyGroupTwo.Contains(this)))
            {
                WhatShouldIDo(EnemyState.Surrounding);
            }
            else
            {
                WhatShouldIDo();
            }
        }
        else
        {
            WhatShouldIDo();
        }
    }

    #endregion

    #region Private methods
    private void GetTarget()
    {
        _distanceToOne = (_playerOne.position - transform.position).magnitude;
        _distanceToTwo = (_playerTwo.position - transform.position).magnitude;

        if (_distanceToOne < _distanceToTwo)
        {
            Target = _playerOne;
        }
        else
        {
            Target = _playerTwo;
        }
    }

    private void DamageTaken(int damage)
    {
        StopEverythingMethod();
        Rb.velocity = Vector3.zero;

        Animator.SetTrigger("Hit"); // play animation
        Instantiate(_hitFXPrefab, _self.position, Quaternion.identity);
        Health -= damage;
        CheckHealth();

        WhatShouldIDo();
    }

    private void CheckHealth()
    {
        if (Health <= 0)
        {
            Instantiate(_destroyedFXPrefab, _self.position, Quaternion.identity);
            GameManager.i.enemyManager.enemies.Remove(this);
            Destroy(_self.gameObject);
        }
    }

    private void StopEverythingMethod()
    {
        StopAllCoroutines();
        if (GameManager.i.enemyManager.enemyCurrentlyAttacking == this.gameObject) { GameManager.i.enemyManager.enemyCurrentlyAttacking = null; }
        Animator.SetTrigger("Reset");

    }
    #endregion

}
