using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArmAttack : MonoBehaviour
{
    public EnemyBehaviour spawnParent;
    Collider meleeCollider;
    public int attackDamage;
    public GameObject plane;

    void Awake()
    {
        meleeCollider = GetComponent<Collider>();
        plane = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = spawnParent.attackHitBoxCenterPoint.position;
        transform.LookAt(transform.position + spawnParent.transform.forward);
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
        Vector3 i_colliderSize = meleeCollider.bounds.size;
        Collider[] i_hitColliders = Physics.OverlapBox(transform.position, i_colliderSize/4, transform.rotation); // DOesn't rotate the overlapbox
        int i = 0;
        while (i < i_hitColliders.Length)
        {
            IHitable i_potentialHitableObject = i_hitColliders[i].GetComponentInParent<IHitable>();
            if (i_potentialHitableObject != null)
            {
                Debug.Log("Touched with enemy at " + (i_hitColliders[i].transform.position - transform.position).magnitude + " from player, alors que bound est à " + i_colliderSize.z/4);
                i_potentialHitableObject.OnHit(null, (i_hitColliders[i].transform.position - transform.position).normalized, null, attackDamage, DamageSource.EnemyContact);
            }
            i++;
        }
        ToggleArmCollider(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, meleeCollider.bounds.size / 2);
    }
}
