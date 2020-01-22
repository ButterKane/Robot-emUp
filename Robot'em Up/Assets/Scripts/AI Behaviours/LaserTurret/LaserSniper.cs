using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSniper : MonoBehaviour
{
    public Rigidbody rb;
    public float speed;
    public int damageDealt;
    public float maxLifeTime;
    Vector3 initialPosition;
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform spawnParent;
    private LaserSniperBehaviour enemyScript;
    public GameObject impactFX;
    public Vector3 impactFXScale;
    public LayerMask layerToCheck;
    public float distanceToRaycast;
    public bool canHitEnemies;

    [HideInInspector] public bool isAimingPlayer;
    public float distanceAoEDamage;

    private void Start()
    {
        initialPosition = transform.position;
        enemyScript = spawnParent.GetComponent<LaserSniperBehaviour>();
    }
    
    void Update()
    {
        //rb.velocity = (target.position - transform.position).normalized * speed;
        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0 || !target.GetComponent<PawnController>().IsTargetable())
        {
            Destroy(gameObject);
        }

        RaycastHit hit;
        if(Physics.Raycast(transform.position, rb.velocity, out hit, distanceToRaycast, layerToCheck)){
            GameObject i_impactFX = Instantiate(impactFX, hit.point, Quaternion.identity);
            i_impactFX.transform.localScale = impactFXScale;
            Destroy(i_impactFX, .25f);
            if (isAimingPlayer && Vector3.Distance(hit.point, target.position) < distanceAoEDamage)
            {
                target.GetComponent<PawnController>().Damage(damageDealt);
            }
            Destroy(gameObject);
        }

        //UpdateLaserPosition();
    }

    //public void UpdateLaserPosition()
    //{
    //    transform.position = enemyScript.bulletSpawn.position;
    //    transform.LookAt(transform.position + spawnParent.transform.forward);
    //}

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.tag == "Enemy" && canHitEnemies)
        {
            _other.GetComponent<PawnController>().Damage(damageDealt);
            GameObject i_impactFX = Instantiate(impactFX, transform.position, Quaternion.identity);
            i_impactFX.transform.localScale = impactFXScale;
            Destroy(i_impactFX, 1);
        }
        else if (_other.tag == "Player")
        {
            _other.GetComponent<PawnController>().Damage(damageDealt);
            GameObject i_impactFX = Instantiate(impactFX, transform.position, Quaternion.identity);
            i_impactFX.transform.localScale = impactFXScale;
            Destroy(i_impactFX, 1);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // TODO: check the first touched object. 
        //If this object is hitable, stops the laser at that length
        //Apply force to push back hit object
        //Apply damages (must be written as damages per second, and so divided by 60 when used to match the frame update)
    }
}