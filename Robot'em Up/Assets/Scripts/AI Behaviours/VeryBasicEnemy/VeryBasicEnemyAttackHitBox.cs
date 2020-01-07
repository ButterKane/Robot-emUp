using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeryBasicEnemyAttackHitBox : MonoBehaviour
{
    void OnTriggerEnter(Collider _other)
    {
        Vector3 internal_impactVector = _other.transform.position - transform.position;
        Vector3 internal_flattedDownImpactVector = new Vector3(internal_impactVector.x, 0, internal_impactVector.z);

        if (_other.tag == "Player")
        {
            if (_other.GetComponent<PawnController>() != null)
                _other.GetComponent<PawnController>().Damage(transform.parent.GetComponent<EnemyBehaviour>().damage);

            EnemyShield internal_enemyShield = GetComponentInParent<EnemyShield>();
            if (internal_enemyShield !=null)
            {
                internal_enemyShield.StopAttack();

                if (_other.GetComponent<PawnController>() != null && _other.GetComponent<DunkController>() != null && _other.GetComponent<DunkController>().isDunking() == false)
                    _other.GetComponent<PawnController>().BumpMe(10,1,1, internal_flattedDownImpactVector.normalized, internal_enemyShield.BumpOtherDistanceMod, internal_enemyShield.BumpOtherDurationMod, internal_enemyShield.BumpOtherRestDurationMod);
            }
        }
    }
}
