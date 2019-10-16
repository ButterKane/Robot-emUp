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
    public float pushForce = 2;

    public bool followPlayerOne;
    public bool followPlayerTwo;

    public Animator animator;

    //DEBUG
    public GameObject surrounder;

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
            // TODO: deal damage
            collision.gameObject.GetComponent<PlayerController>().Push((collision.GetContact(0).point - self.position).normalized, pushForce);
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

            float newDistance = (p2 -p0).magnitude;

            if (t < 1)
            {
                t += Time.deltaTime/(newDistance *25) ;
            }
            yield return null;
        } while ((p0 - p2).magnitude > 0.1f);
        
        yield return null;
    }


}
