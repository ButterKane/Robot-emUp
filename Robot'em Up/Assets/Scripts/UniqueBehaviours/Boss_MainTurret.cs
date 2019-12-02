using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_MainTurret : MonoBehaviour
{
    public float speed;
    public float multiplier;
    public int DamageToPlayer;
    // Start is called before the first frame update
    void Start()
    {
        multiplier = 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Boss_Manager.i.OnePlayerLeft)
        {

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + Time.deltaTime * speed * Boss_Manager.i.difficulty * multiplier * 1.3f, transform.rotation.eulerAngles.z);
        }
        else
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + Time.deltaTime * speed * Boss_Manager.i.difficulty * multiplier, transform.rotation.eulerAngles.z);

        }
    }

    public void InverseLaser()
    {
        if (multiplier ==-1)
        {
            multiplier = 1;
        }
        else
        {
            multiplier = -1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PawnController>())
        {
            
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            pawn.Damage(DamageToPlayer);
        }


        if (other.gameObject.GetComponent<EnemyBehaviour>())
        {

            EnemyBehaviour ennemy = other.gameObject.GetComponent<EnemyBehaviour>();
            ennemy.OnHit(null, Vector3.zero, null, DamageToPlayer, DamageSource.RedBarrelExplosion);
            
        }

    }
}
