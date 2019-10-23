using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public Transform self;
    public Rigidbody rb;
    public ParticleSystem hitFXPrefab;
    public ParticleSystem destroyedFXPrefab;
    public EnemyAttack attackScript;
    public Animator animator;

    [Space(2)]
    [Header("Auto-assigned References")]
    public Transform target;
    public Transform playerOne;
    public Transform playerTwo;

    [Space(2)]
    [Header("Variables")]
    public EnemyState state;

    public bool isAttacking = false;

    public int maxHealth = 100;
    public int health;

    public float attackDistance = 7f;
    public float pushForce = 300;

    public bool followPlayer;
    public float followSpeed = 100f;
    public float timeBeforeSurround = 2f;

    private float distanceToOne;
    private float distanceToTwo;

    [Space(2)]
    [Header("Debug")]
    public GameObject surrounder;

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
        playerOne = GameManager.i.playerOne.transform;
        playerTwo = GameManager.i.playerTwo.transform;

        health = maxHealth;
        followPlayer = false;
        state = EnemyState.Idle;
        GameManager.i.enemyManager.enemies.Add(this);

        WhatShouldIDo(EnemyState.Idle);
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    GetTarget();

    //    //if (Input.GetKeyDown(KeyCode.P))
    //    //{
    //    //    DamageTaken(10);
    //    //}

    //    if (state == EnemyState.Following)
    //    {
    //        if (target)
    //        {
    //            FollowPlayer(target);
    //        }
    //    }

    //    if (state == EnemyState.Attacking)
    //    {
    //        if (!isAttacking)
    //        {
    //            LaunchAttack(target);
    //            isAttacking = true;
    //        }
    //    }

    //    if (state == EnemyState.Idle)
    //    {
    //        WhatShouldIDo();
    //    }
    //}



    public void WhatShouldIDo(EnemyState priorityState = EnemyState.Null)
    {
        if (priorityState != EnemyState.Null)
        {
            state = priorityState;
        }
        else
        {
            GetTarget();

            if ((target.position - self.position).magnitude < attackDistance)
            {
                if (GameManager.i.enemyManager.enemyCurrentlyAttacking == null)
                {
                    GameManager.i.enemyManager.enemyCurrentlyAttacking = self.gameObject;
                    state = EnemyState.Attacking;
                }
                else
                {
                    state = EnemyState.Idle;
                }
            }
            else if (target != null)
            {
                state = EnemyState.Following;
            }
        }
        
        // Compute the choice according to the state
        switch (state)
        {
            case EnemyState.Idle:
                StartCoroutine(WaitABit());
                break;

            case EnemyState.Moving:
                // Simple move in an area, not following a player
                break;

            case EnemyState.Following:
                StartCoroutine(FollowPlayer(target));
                break;

            case EnemyState.Attacking:
                LaunchAttack(target);
                break;

            case EnemyState.Surrounding:
                LaunchSurrounding();
                break;

            default:
                state = EnemyState.Idle; // Making sure the enemy never "bugs" and ends up with no state
                break;
        }
    }


    public void DamageTaken(int damage)
    {
        StopEverythingMethod();
        rb.velocity = Vector3.zero;

        animator.SetTrigger("Hit"); // play animation
        Instantiate(hitFXPrefab, self.position, Quaternion.identity);
        health -= damage;
        CheckHealth();

        WhatShouldIDo();
    }

    public void CheckHealth()
    {
        if (health <= 0)
        {
            Instantiate(destroyedFXPrefab, self.position, Quaternion.identity);
            GameManager.i.enemyManager.enemies.Remove(this);
            Destroy(self.gameObject);
        }
    }

    public IEnumerator FollowPlayer(Transform playerToFollow)
    {
        animator.SetBool("FollowingPlayer", true);
        float followTime = 0;

        while ((target.position - self.position).magnitude > attackDistance && followTime <= timeBeforeSurround) 
        {
            self.LookAt(SwissArmyKnife.GetFlattedDownPosition(playerToFollow.position, self.position));

            rb.velocity = Vector3.zero;

            rb.AddForce(self.forward * followSpeed * Time.deltaTime, ForceMode.VelocityChange);

            followTime += Time.deltaTime;

            yield return null;
        }
        
        if (followTime >= timeBeforeSurround)
        {
            WhatShouldIDo(EnemyState.Surrounding);
        }
        else
        {
            WhatShouldIDo();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (collision.gameObject.GetComponent<PlayerController>().isInvincible == false)
            {
                Vector3 newCollisionPoint = new Vector3(collision.GetContact(0).point.x, collision.gameObject.transform.position.y, collision.GetContact(0).point.z); // Make sure the impact is "leveled" and not with a y angle
                collision.gameObject.GetComponent<PlayerController>().Push((newCollisionPoint - self.position).normalized, pushForce, newCollisionPoint);
                collision.gameObject.GetComponent<PlayerController>().DamagePlayer(10);
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

    public void LaunchSurrounding()
    {
        Debug.Log("target of " + self.name + " is " + target);

        if (GameManager.i.enemyManager.surrounderInstance != null)
        {
            surrounder = GameManager.i.enemyManager.surrounderInstance;
        }
        else
        {
            surrounder = GameManager.i.enemyManager.SpawnSurrounderInstance(target.position);
        }

        StartCoroutine(SurroundPlayer(target.gameObject));
    }

    public IEnumerator SurroundPlayer(GameObject player)
    {
        Surrounder script = surrounder.GetComponent<Surrounder>();
        surrounder.transform.position = player.transform.position;

        // Bezier quadratic curve : (1-avancement)^2*p0 + 2(1-avancement)*avancement*p1 + avancement^2*p2
        // With p0,p1,p2 as Vector3 positions, p0 & p2 as start an end points

        Transform surroundingPoint = script.GetSurroundingPoint();
        Vector3 p0 = self.position;
        Vector3 p2 = script.GetAPositionFromPoint(surroundingPoint);

        int moveSens = Vector3.SignedAngle(p2 - p0, player.transform.position - p0, Vector3.up) > 1 ? 1 : -1;

        Vector3 p1 = p0 + (p2 - p0) / 0.5f + Vector3.Cross(p2 - p0, Vector3.up) * moveSens * 0.5f;
        float distance = (p2 - p0).magnitude;

        float t = 0;
        do
        {
            p0 = self.position;
            p2 = script.GetAPositionFromPoint(surroundingPoint);
            p2 = new Vector3(p2.x, self.position.y, p2.z);
            p1 = p0 + (p2 - p0) / 0.5f + Vector3.Cross(p2 - p0, Vector3.up) * moveSens * 0.5f;

            Vector3 pNow = (Mathf.Pow((1 - t), 2) * p0) + (2 * (1 - t) * t * p1) + (Mathf.Pow(t, 2) * p2);

            self.position = pNow;

            float newDistance = (p2 - p0).magnitude;

            if (t < 1)
            {
                t += Time.deltaTime / (newDistance * 25);
            }
            yield return null;
        } while ((p0 - p2).magnitude > 0.1f);

        yield return null;

        WhatShouldIDo();
    }

    public void GetTarget()
    {
        distanceToOne = (playerOne.position - transform.position).magnitude;
        distanceToTwo = (playerTwo.position - transform.position).magnitude;

        if (distanceToOne < distanceToTwo)
        {
            target = playerOne;
        }
        else
        {
            target = playerTwo;
        }
    }

    public void LaunchAttack(Transform target)
    {
        transform.LookAt(SwissArmyKnife.GetFlattedDownPosition(target.position, self.position));
        StartCoroutine(attackScript.JumpAttack(target));
        animator.SetTrigger("PrepareAttack");
    }

    public IEnumerator WaitABit()
    {
        state = EnemyState.Idle;
        yield return new WaitForSeconds(1.5f);
        WhatShouldIDo();
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PlayerController _thrower, int _damages, DamageSource _source)
    {
        Debug.Log("Damage taken " + _source);
        DamageTaken(_damages);
        MomentumManager.IncreaseMomentum(0.1f);

        StopEverythingMethod();

        WhatShouldIDo();
    }

    public void StopEverythingMethod()
    {
        StopAllCoroutines();
        if (GameManager.i.enemyManager.enemyCurrentlyAttacking == this.gameObject) { GameManager.i.enemyManager.enemyCurrentlyAttacking = null; }
        animator.SetTrigger("Reset");

    }
}
