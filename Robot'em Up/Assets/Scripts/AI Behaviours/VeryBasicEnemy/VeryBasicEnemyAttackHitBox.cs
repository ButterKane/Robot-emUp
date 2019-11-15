using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeryBasicEnemyAttackHitBox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        print(other.name);
        if (other.tag == "Player")
        {
            if (other.GetComponent<PawnController>() != null)
                other.GetComponent<PawnController>().Damage(2);
        }
    }
}
