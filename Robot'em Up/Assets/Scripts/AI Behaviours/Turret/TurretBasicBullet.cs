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
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        rb.position = transform.position + transform.forward * speed * Time.deltaTime;
        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        //print(other.tag);
        if(_other.tag == "Player")
        {
            _other.GetComponent<PawnController>().Damage(damageDealt);
            Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
            Destroy(gameObject);
        }
        else if(_other.tag == "Environment")
        {
            Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
            Destroy(gameObject);
        }
    }
}
