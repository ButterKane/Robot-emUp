using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArmAttack : MonoBehaviour
{
    public EnemyBehaviour spawnParent;
    Collider meleeCollider;
    public int attackDamage;
    public GameObject mainPlane;
    public GameObject highlightPlane;

    void Awake()
    {
        meleeCollider = GetComponent<Collider>();
        mainPlane = transform.GetChild(0).gameObject;
        highlightPlane = mainPlane.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(spawnParent.attackHitBoxCenterPoint.position.x, spawnParent.attackHitBoxCenterPoint.position.y + spawnParent.attackHitBoxCenterPoint.localScale.y / 2, spawnParent.attackHitBoxCenterPoint.position.z);
        transform.LookAt(transform.position + spawnParent.transform.forward);
    }

    public void ToggleArmCollider(bool? value)
    {
        if (value != null)
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
        Collider[] i_hitColliders = Physics.OverlapBox(transform.position, meleeCollider.bounds.size/2, Quaternion.identity);
        int i = 0;
        while (i < i_hitColliders.Length)
        {
            IHitable i_potentialHitableObject = i_hitColliders[i].GetComponent<IHitable>();
            if (i_potentialHitableObject != null)
            {
                i_potentialHitableObject.OnHit(null, (other.transform.position - transform.position).normalized, null, attackDamage, DamageSource.EnemyContact);
            }
            i++;
        }
        ToggleArmCollider(false);
    }
}
