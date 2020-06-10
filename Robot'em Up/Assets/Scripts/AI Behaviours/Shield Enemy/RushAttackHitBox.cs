using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushAttackHitBox : MonoBehaviour
{
    public EnemyShield parent;
    Collider contactCollider;

    void Awake()
    {
        contactCollider = GetComponent<Collider>();
    }

    public void ToggleCollider(bool? value)
    {
        if (value != null)
        {
            contactCollider.enabled = (bool)value;
        }
        else
        {
            contactCollider.enabled = !contactCollider.enabled;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            other.GetComponentInParent<IHitable>().OnHit(null, other.transform.position - transform.position, null, parent.damage, DamageSource.EnemyContact);
            FeedbackManager.SendFeedback("event.EnemyShieldHitPlayer", this);
            parent.AttackTouchedSoStop();
            ToggleCollider(false);
        }
        else if (other.transform.tag == "Enemy")
        {
            //other.GetComponent<EnemyBehaviour>().rb.AddExplosionForce(5, spawnParent.transform.position, 1);
        }

        
    }
}
