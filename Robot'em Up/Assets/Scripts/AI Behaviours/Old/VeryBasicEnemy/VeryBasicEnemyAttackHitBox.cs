using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeryBasicEnemyAttackHitBox : MonoBehaviour
{
    void OnTriggerEnter(Collider _other)
    {
        Vector3 i_impactVector = _other.transform.position - transform.position;
        Vector3 i_flattedDownImpactVector = new Vector3(i_impactVector.x, 0, i_impactVector.z);

        if (_other.tag == "Player")
        {
            if (_other.GetComponent<PawnController>() != null)
                _other.GetComponent<PawnController>().Damage(transform.parent.GetComponent<EnemyBehaviour>().damage);

            EnemyShield i_enemyShield = GetComponentInParent<EnemyShield>();
            if (i_enemyShield !=null)
            {
                i_enemyShield.AttackTouchedSoStop();

                if (_other.GetComponent<PawnController>() != null && _other.GetComponent<DunkController>() != null && _other.GetComponent<DunkController>().IsDunking() == false)
                    _other.GetComponent<PawnController>().Push(PushType.Light, i_flattedDownImpactVector.normalized,PushForce.Force2);
            }
        }
    }
}
