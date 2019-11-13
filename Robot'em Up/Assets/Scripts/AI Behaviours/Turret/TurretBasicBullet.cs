using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBasicBullet : MonoBehaviour
{
    public Rigidbody rb;
    public float speed;
    public GameObject deathParticle;

    // Update is called once per frame
    void Update()
    {
        rb.AddForce(transform.forward * speed * Time.deltaTime, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.GetComponent<PawnController>().Damage(1);
            Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
            Destroy(gameObject);
        }
    }
}
