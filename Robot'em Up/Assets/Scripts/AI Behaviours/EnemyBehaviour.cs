﻿using System.Collections;
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
public class EnemyBehaviour : MonoBehaviour, IHitable
{
    [Header("References")]
    [SerializeField] private Transform _self;
    public Rigidbody Rb;
    [SerializeField] private ParticleSystem _hitFXPrefab;
    [SerializeField] private ParticleSystem _destroyedFXPrefab;
    [SerializeField] private EnemyAttack _attackScript;
    public Animator Animator;

    [Space(2)]
    [Header("Auto-assigned References")]
    public Transform Target;
    [SerializeField] private Transform _playerOne;
    [SerializeField] private Transform _playerTwo;

    [Space(2)]
    [Header("Variables")]
    public EnemyState State;

    public bool IsAttacking = false;

    public int MaxHealth = 100;
    public int Health;

    public float AttackDistance = 7f;
    public float PushForce = 300;

    public bool IsFollowingPlayer;
    public float FollowSpeed = 100f;
    public float TimeBeforeSurround = 2f;

    private float _distanceToOne;
    private float _distanceToTwo;

    [Space(2)]
    [Header("Debug")]
    [SerializeField] private GameObject _surrounder;

    private int _hitCount;
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

        WhatShouldIDo(EnemyState.Idle);
        _self = transform;
    }

    public void WhatShouldIDo(EnemyState priorityState = EnemyState.Null)
    {
        if (priorityState != EnemyState.Null)
        {
            State = priorityState;
        }
        else
        {
            GetTarget();

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

        // Compute the choice according to the state
        switch (State)
        {
            case EnemyState.Idle:
                StartCoroutine(WaitABit());
                break;

            case EnemyState.Moving:
                // Simple move in an area, not following a player
                break;

            case EnemyState.Following:
                StartCoroutine(FollowPlayer(Target));
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
    public void LaunchSurrounding()
    {
        //Debug.Log("target of " + self.name + " is " + target);

        if (GameManager.i.enemyManager.surrounderInstance != null)
        {
            _surrounder = GameManager.i.enemyManager.surrounderInstance;
        }
        else
        {
            _surrounder = GameManager.i.enemyManager.SpawnSurrounderInstance(Target.position);
        }

        StartCoroutine(SurroundPlayer(Target.gameObject));
    }

    public void LaunchAttack(Transform target)
    {
        transform.LookAt(SwissArmyKnife.GetFlattedDownPosition(target.position, _self.position));
        StartCoroutine(_attackScript.Attack(target));
        Animator.SetTrigger("PrepareAttack");
    }
    #endregion

    #region Coroutines
    public IEnumerator WaitABit()
    {
        State = EnemyState.Idle;
        yield return new WaitForSeconds(1.5f);
        WhatShouldIDo();
    }

    public IEnumerator SurroundPlayer(GameObject player)
    {
        float distanceToNextPoint = 0f;
        Surrounder script = _surrounder.GetComponent<Surrounder>();
        _surrounder.transform.position = player.transform.position;

        // Bezier quadratic curve : (1-avancement)^2*p0 + 2(1-avancement)*avancement*p1 + avancement^2*p2
        // With p0,p1,p2 as Vector3 positions, p0 & p2 as start an end points

        Transform surroundingPoint = script.GetSurroundingPoint();
        Vector3 p0 = _self.position;
        Vector3 p2 = script.GetAPositionFromPoint(surroundingPoint);
        p2 = SwissArmyKnife.GetFlattedDownPosition(p2, _self.position);

        int moveSens = Vector3.SignedAngle(p2 - p0, player.transform.position - p0, Vector3.up) > 1 ? 1 : -1;

        Vector3 p1 = p0 + (p2 - p0) / 0.5f + Vector3.Cross(p2 - p0, Vector3.up) * moveSens * 0.5f;
        float distance = (p2 - p0).magnitude;

        float t = 0;
        float timeNotMovingMuch = 0f;

        do
        {
            p0 = _self.position;
            p1 = p0 + (p2 - p0) / 0.5f + Vector3.Cross(p2 - p0, Vector3.up) * moveSens * 0.5f;

            Vector3 pNow = (Mathf.Pow((1 - t), 2) * p0) + (2 * (1 - t) * t * p1) + (Mathf.Pow(t, 2) * p2);
            
            _self.position = pNow;

            distanceToNextPoint = (pNow - p0).magnitude;

            float newDistance = (p2 - p0).magnitude;

            if ((_self.position- p0).magnitude < distanceToNextPoint)    // Comparing the traveled distance and the supposed distance to see if blocked
            {
                timeNotMovingMuch += Time.deltaTime;
            }
            else
            {
                timeNotMovingMuch = 0f;
            }

            if (t < 1)
            {
                t += Time.deltaTime / (newDistance * 25);
            }
            yield return null;
        } while ((p0 - p2).magnitude > 0.1f && timeNotMovingMuch < 0.5f);

        if (timeNotMovingMuch > 0.5f)
        {
            Debug.Log("stopped because not moving enough");
        }
        yield return null;

        WhatShouldIDo();
    }

    public IEnumerator FollowPlayer(Transform playerToFollow)
    {
        Animator.SetBool("FollowingPlayer", true);
        float followTime = 0;

        while ((Target.position - _self.position).magnitude > AttackDistance && followTime <= TimeBeforeSurround)
        {
            _self.LookAt(SwissArmyKnife.GetFlattedDownPosition(playerToFollow.position, _self.position));

            Rb.velocity = Vector3.zero;

            Rb.AddForce(_self.forward * FollowSpeed * Time.deltaTime, ForceMode.VelocityChange);

            followTime += Time.deltaTime;

            yield return null;
        }

        if (followTime >= TimeBeforeSurround)
        {
            WhatShouldIDo(EnemyState.Surrounding);
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
