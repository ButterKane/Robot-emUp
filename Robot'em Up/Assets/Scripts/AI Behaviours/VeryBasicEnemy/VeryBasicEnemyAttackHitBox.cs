using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeryBasicEnemyAttackHitBox : MonoBehaviour
{

    public int damage;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.GetComponent<PawnController>() != null)
                other.GetComponent<PawnController>().Damage(damage);
        }
    }
}
