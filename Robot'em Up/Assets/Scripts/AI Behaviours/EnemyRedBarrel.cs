using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRedBarrel : EnemyBehaviour
{
    [Space(2)]
    [Separator("Red Barrel Death variables")]
    [SerializeField] GameObject explosionFX;
    public float explosionRadius = 3f;
    public int explosionDamage = 10;
    public float buildUpBeforeExplosion = 0.5f;

    protected override void Die()
    {
        StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        Animator.SetTrigger("RedBarrelDeath");
        yield return new WaitForSeconds(buildUpBeforeExplosion);

        GameObject hitParticle = Instantiate(explosionFX, transform.position, Quaternion.identity);
        Destroy(hitParticle, 1f);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        int i = 0;
        while (i < hitColliders.Length)
        {
            IHitable potentialHitableObject = hitColliders[i].GetComponentInParent<IHitable>();
            if (potentialHitableObject != null)
            {
                potentialHitableObject.OnHit(null, (hitColliders[i].transform.position - transform.position).normalized, null, explosionDamage, DamageSource.RedBarrelExplosion);
            }
            i++;
        }

        base.Die();
    }

    
}
