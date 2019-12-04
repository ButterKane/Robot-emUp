using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSniperBullet : MonoBehaviour
{
    public Rigidbody rb;
    public float speed;
    public GameObject deathParticle;
    public int damageDealt;
    public float maxLifeTime;
    Vector3 initialPosition;
    public Transform target;

    private void Start()
    {
        initialPosition = transform.position;
    }
    
    void Update()
    {
        rb.velocity = (target.position - transform.position).normalized * speed;
        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //print(other.tag);
        if (other.tag == "Player")
        {
            other.GetComponent<PawnController>().Damage(damageDealt);
            Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
            Destroy(gameObject);
        }
        else if (other.tag == "Environment")
        {
            Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
            Destroy(gameObject);
        }
    }
}