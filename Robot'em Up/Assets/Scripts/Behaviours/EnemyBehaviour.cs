using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyState
{
    Idle,
    Moving,
    Following,
    Attacking,
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

    public int maxHealth = 100;
    public int health;

    public float attackDistance = 7f;
    public float pushForce = 300;

    public bool followPlayer;
    public float followSpeed = 100f;
    
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
    }

    // Update is called once per frame
    void Update()
    {
        GetTarget();

        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    DamageTaken(10);
        //}

        if (state == EnemyState.Following)
        {
            Debug.Log("following");
            if (target)
            {
                Debug.Log("really following");
                FollowPlayer(target);
            }
        }

        if (state == EnemyState.Attacking)
        {
            if (GameManager.i.enemyManager.enemyCurrentlyAttacking == null)
            {
                GameManager.i.enemyManager.enemyCurrentlyAttacking = self.gameObject;
                LaunchAttack(target);
            }
        }
    }

    public void WhatShouldIDo()
    {
        if ((target.position - self.position).magnitude < attackDistance)
        {
            state = EnemyState.Attacking;
        }
        else if (target != null)
        {
            state = EnemyState.Following;
        }
    }


    public void DamageTaken(int damage)
    {
        animator.SetTrigger("Hit"); // play animation
        Instantiate(hitFXPrefab, self.position, Quaternion.identity);
        health -= damage;
        CheckHealth();
    }

    public void CheckHealth()
    {
        if (health <= 0)
        {
            Instantiate(destroyedFXPrefab, self.position, Quaternion.identity);
            Destroy(self.gameObject);
        }
    }

    public void FollowPlayer(Transform playerToFollow)
    {
        state = EnemyState.Following;
        animator.SetBool("FollowingPlayer", true);

        self.LookAt(playerToFollow.position);

        rb.velocity = Vector3.zero;

        rb.AddForce(self.forward * followSpeed * Time.deltaTime, ForceMode.VelocityChange);

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (collision.gameObject.GetComponent<PlayerController>().isInvincible == false)
            {
                Vector3 newCollisionPoint = new Vector3(collision.GetContact(0).point.x, collision.gameObject.transform.position.y, collision.GetContact(0).point.z); // Make sure the impact is "leveled" and not with a y angle
                collision.gameObject.GetComponent<PlayerController>().Push((newCollisionPoint - self.position).normalized, pushForce, newCollisionPoint);
                collision.gameObject.GetComponent<PlayerController>().DamagePlayer(10);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.i.ball)
        {
            DamageTaken(GameManager.i.ballDamage);
        }
    }

    public IEnumerator SurroundPlayer(GameObject player)
    {
        state = EnemyState.Moving;
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
        state = EnemyState.Idle;
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
        StopAllCoroutines();
        StartCoroutine(attackScript.JumpAttack(target));
        animator.SetTrigger("PrepareAttack");
    }

    public void OnHit(BallBehaviour _ball, Vector3 _impactVector, PlayerController _thrower, int _damages, DamageSource _source)
    {
        Debug.Log("Damage taken " + _source);
        DamageTaken(_damages);
        MomentumManager.IncreaseMomentum(0.1f);
        StopAllCoroutines();
        WhatShouldIDo();
    }
}
