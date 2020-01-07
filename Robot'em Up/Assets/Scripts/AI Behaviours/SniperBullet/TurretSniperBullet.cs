using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSniperBullet : MonoBehaviour
{
    public Rigidbody rb;
    public float speed;
    public int damageDealt;
    public float maxLifeTime;
    Vector3 initialPosition;
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform spawnParent;
    public GameObject impactFX;
    public Vector3 impactFXScale;
    public LayerMask layerToCheck;
    public float distanceToRaycast;

    [HideInInspector] public bool isAimingPlayer;
    public float distanceAoEDamage;

    private void Start()
    {
        initialPosition = transform.position;
    }
    
    void Update()
    {
        rb.velocity = (target.position - transform.position).normalized * speed;
        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0 || !target.GetComponent<PawnController>().IsTargetable())
        {
            Destroy(gameObject);
        }

        RaycastHit hit;
        if(Physics.Raycast(transform.position, rb.velocity, out hit, distanceToRaycast, layerToCheck)){
            GameObject internal_impactFX = Instantiate(impactFX, hit.point, Quaternion.identity);
            internal_impactFX.transform.localScale = impactFXScale;
            Destroy(internal_impactFX, .25f);
            if (isAimingPlayer && Vector3.Distance(hit.point, target.position) < distanceAoEDamage)
            {
                target.GetComponent<PawnController>().Damage(damageDealt);
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        //print(other.tag);
        if (_other.tag == "Player")
        {
            _other.GetComponent<PawnController>().Damage(damageDealt);
            GameObject internal_impactFX = Instantiate(impactFX, transform.position, Quaternion.identity);
            internal_impactFX.transform.localScale = impactFXScale;
            Destroy(internal_impactFX, 1);
            Destroy(gameObject);
        }
    }
}