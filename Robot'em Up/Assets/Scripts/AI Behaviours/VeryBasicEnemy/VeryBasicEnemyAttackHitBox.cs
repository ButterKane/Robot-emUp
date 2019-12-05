using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeryBasicEnemyAttackHitBox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Vector3 impactVector = other.transform.position - transform.position;
        Vector3 flattedDownImpactVector = new Vector3(impactVector.x, 0, impactVector.z);

        if (other.tag == "Player")
        {
            if (other.GetComponent<PawnController>() != null)
                other.GetComponent<PawnController>().Damage(transform.parent.GetComponent<EnemyBehaviour>().damage);

            EnemyShield enemyShield = GetComponentInParent<EnemyShield>();
            if (enemyShield !=null)
            {
                enemyShield.StopAttack();

                if (other.GetComponent<PawnController>() != null)
                    other.GetComponent<PawnController>().BumpMe(10,1,1, flattedDownImpactVector.normalized, enemyShield.BumpOtherDistanceMod, enemyShield.BumpOtherDurationMod, enemyShield.BumpOtherRestDurationMod);
            }
        }
    }
}
