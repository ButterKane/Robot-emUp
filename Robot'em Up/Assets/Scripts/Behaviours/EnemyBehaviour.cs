using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public Transform self;
    public Rigidbody rb;

    public Transform playerOne;
    public Transform playerTwo;

    public int maxHealth = 100;
    public int health;

    public float followSpeed = 100f;

    public bool followPlayerOne;
    public bool followPlayerTwo;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        self = transform;

        playerOne = GameManager.i.playerOne.transform;
        playerTwo = GameManager.i.playerTwo.transform;

        health = maxHealth;
        followPlayerOne = false;
        followPlayerTwo = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            DamageTaken(10);
        }

        if (followPlayerOne)
        {
            if(followPlayerTwo) { followPlayerOne = false; }
            if (playerOne)
            {
                FollowPlayer(playerOne);
            }
        }

        if(followPlayerTwo)
        {
            if (followPlayerOne) { followPlayerTwo = false; }
            if (playerTwo)
            {
                FollowPlayer(playerTwo);
            }
        }
    }

    public void DamageTaken(int damage)
    {
        animator.SetTrigger("Hit"); // play animation
        health -= damage;
        CheckHealth();
    }

    public void CheckHealth()
    {
        if (health <= 0)
        {
            // TODO Dead
        }
    }

    public void FollowPlayer(Transform playerToFollow)
    {
        animator.SetBool("FollowingPlayer", true);
        Vector3 targetPosition = playerToFollow.position;
        
        self.LookAt(targetPosition);

        float distanceToTarget = (targetPosition - self.position).magnitude;

        Vector3 moveDirection = Vector3.zero;
        rb.velocity = Vector3.zero;

        rb.AddForce(self.forward * followSpeed * Time.deltaTime, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            DealDamage();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.i.ball)
        {
            DamageTaken(GameManager.i.ballDamage);
        }
    }

    private void DealDamage()
    {
        Debug.Log("Damaging player");
        // TODO: deal damage to player
    }
}
