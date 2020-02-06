using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBasicBullet : MonoBehaviour
{
    public Rigidbody rb;
    public Transform launcher;
    public float speed;
    public GameObject deathParticle;
    public int damageDealt;
    public float maxLifeTime;
    public Transform target;
    public bool canHitEnemies;
    public float damageModificator;

    // Update is called once per frame

        
    void Update()
    {
        rb.position = transform.position + transform.forward * speed * Time.deltaTime;
        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0)
        {
            Debug.Log("maxlifetime turret");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.transform != launcher)
        {
            if (_other.transform.root.tag == "Enemy" && canHitEnemies && _other.transform != launcher)
            {
                _other.GetComponent<PawnController>().Damage(Mathf.RoundToInt(damageDealt * damageModificator));
                Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
                Destroy(gameObject);
            }
            else if (_other.tag == "Player")
            {
                _other.GetComponent<PlayerController>().Damage(damageDealt);
                Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
                Destroy(gameObject);
            }
            else if (_other.tag == "Environment")
            {
                Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
                Destroy(gameObject);
            }
            else if (_other.tag == "Boss_Destructible")
            {
                Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
                Destroy(gameObject);
            }
        }
    }
}
