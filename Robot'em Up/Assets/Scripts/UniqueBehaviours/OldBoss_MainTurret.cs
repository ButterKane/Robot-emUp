using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OldBoss_MainTurret : MonoBehaviour
{
    public float speed;
    public float multiplier;
    public int damageToPlayer;
    public bool selfRotating = true;
    // Start is called before the first frame update
    void Start()
    {
        multiplier = 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (selfRotating)
        {
        if (OldBoss_Manager.i.OnePlayerLeft)
        {

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + Time.deltaTime * speed * OldBoss_Manager.i.difficulty * multiplier * 1.3f, transform.rotation.eulerAngles.z);
        }
        else
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + Time.deltaTime * speed * OldBoss_Manager.i.difficulty * multiplier, transform.rotation.eulerAngles.z);

            }

        }
    }

    public void InverseLaser()
    {
		OldBoss_Manager.i.inversionMessage.gameObject.SetActive(true);
		OldBoss_Manager.i.showInversionMessage = 2f;
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
            pawn.Damage(damageToPlayer);
        }


        if (other.gameObject.GetComponent<EnemyBehaviour>())
        {

            EnemyBehaviour ennemy = other.gameObject.GetComponent<EnemyBehaviour>();
            ennemy.OnHit(null, Vector3.zero, null, damageToPlayer, DamageSource.RedBarrelExplosion);
            
        }

    }
}
