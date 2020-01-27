using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossArmAttack : MonoBehaviour
{
    Collider meleeCollider;
    public int attackDamage;
    public GameObject plane;

    void Awake()
    {
        meleeCollider = GetComponent<Collider>();
        plane = transform.GetChild(0).gameObject;
        attackDamage = GetComponentInParent<EnemyBoss>().PunchAttack_DamageInflicted;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleArmCollider(bool? value)
    {
        if(value != null)
        {
            meleeCollider.enabled = (bool)value;
        }
        else
        {
            meleeCollider.enabled = !meleeCollider.enabled;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 colliderSize = meleeCollider.bounds.size;
        Collider[] i_hitColliders = Physics.OverlapBox(transform.position, colliderSize/2, Quaternion.identity);
        int i = 0;
        while (i < i_hitColliders.Length)
        {
            IHitable i_potentialHitableObject = i_hitColliders[i].GetComponentInParent<IHitable>();
            if (i_potentialHitableObject != null && i_hitColliders[i].gameObject.tag == "Player")
            {
                i_potentialHitableObject.OnHit(null, (i_hitColliders[i].transform.position - transform.position).normalized, null, attackDamage, DamageSource.EnemyContact);
            }
            if (i_hitColliders[i].GetComponent<NavMeshObstacle>())
            {
                Destroy(i_hitColliders[i].gameObject);
            }
            i++;
        }
        ToggleArmCollider(false);
    }
}
