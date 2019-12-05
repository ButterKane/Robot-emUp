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
            GameObject _impactFX = Instantiate(impactFX, transform.position, Quaternion.identity);
            _impactFX.transform.localScale = impactFXScale;
            Destroy(_impactFX, 1);
            Destroy(gameObject);
        }
        else if (other.tag == "Environment")
        {
            GameObject _impactFX = Instantiate(impactFX, transform.position, Quaternion.identity);
            _impactFX.transform.localScale = impactFXScale;
            Destroy(_impactFX, .25f);
            Destroy(gameObject);
        }
    }
}