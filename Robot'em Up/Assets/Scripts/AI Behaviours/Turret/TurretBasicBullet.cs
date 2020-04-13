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
    private float currentLifeTime;
    public Transform target;
    public bool canHitEnemies;
    public float damageModificator;

    // Update is called once per frame

        
    void Update()
    {
        rb.position = transform.position + transform.forward * speed * Time.deltaTime;
        currentLifeTime -= Time.deltaTime;
        if (currentLifeTime <= 0)
        {
            Debug.Log("maxlifetime turret");
            gameObject.SetActive(false);
        }
    }

    public void ActivateProjectile(Vector3 _positionToSpawn, Quaternion _rotationToDirection)
    {
        currentLifeTime = maxLifeTime;
        transform.position = _positionToSpawn;
        transform.rotation = _rotationToDirection;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.transform != launcher)
        {
            if (_other.transform.root.tag == "Enemy" && canHitEnemies && _other.transform != launcher)
            {
                _other.GetComponent<PawnController>().Damage(Mathf.RoundToInt(damageDealt * damageModificator));
                Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
                gameObject.SetActive(false);
            }
            else if (_other.tag == "Player")
            {
                _other.GetComponent<PlayerController>().Damage(damageDealt);
                Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
                gameObject.SetActive(false);
            }
            else if (_other.tag == "Environment" || _other.tag == "Ground")
            {
                Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
                gameObject.SetActive(false);
            }
            else if (_other.tag == "Boss_Destructible")
            {
                Destroy(Instantiate(deathParticle, transform.position, Quaternion.identity), .25f);
                gameObject.SetActive(false);
            }
        }
    }
}
