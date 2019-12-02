using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBasicBullet : MonoBehaviour
{
    public Rigidbody rb;
    public float speed;
    public GameObject deathParticle;
    public int damageDealt;
    public float maxLifeTime;

    // Update is called once per frame
    void Update()
    {
        rb.position = transform.position + transform.forward * speed * Time.deltaTime;
        //rb.AddForce(transform.forward * speed * Time.deltaTime, ForceMode.VelocityChange);
        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //print(other.tag);
        if(other.tag == "Player")
        {
            other.GetComponent<PawnController>().Damage(damageDealt);
            Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
            Destroy(gameObject);
        }
        else if(other.tag == "Environment")
        {
            Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
            Destroy(gameObject);
        }
    }
}
