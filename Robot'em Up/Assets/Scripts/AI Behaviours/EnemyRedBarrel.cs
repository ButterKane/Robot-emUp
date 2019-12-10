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

    [Separator("Explosion Bump variables")]
    public float BumpDistanceMod = 1.5f;
    public float BumpDurationMod = 0.7f;
    public float BumpRestDurationMod = 0.2f;
    private Vector3 bumpValues;

    public Renderer explosionRadiusRenderer;
    public Transform explosionGrowingRenderer;
    public Transform explosionRadiusTransform;
    public Renderer bodyRenderer;
    public Material materialOnExplosion;
    public Color emissionColor;
    public Vector2 minMaxEmissionOnDeath;

    new void Start()
    {
        base.Start();
        bumpValues = new Vector3(BumpDistanceMod, BumpDurationMod, BumpRestDurationMod);
        explosionRadiusTransform.localScale = new Vector3(explosionRadius * 2, explosionRadius * 2, explosionRadius * 2);
    }

    public override void PreparingAttackState()
    {
        ChangingState(EnemyState.Dying);
        bodyRenderer.material = materialOnExplosion;
    }

    protected override void Die()
    {
        StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        Animator.SetTrigger("DeathTrigger");
        SoundManager.PlaySound("RedBarrelExplosionAnticipation", transform.position, transform);
        explosionRadiusTransform.gameObject.SetActive(true);
        float t = 0;
        while (t < 1)
        {
            explosionGrowingRenderer.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            t += Time.deltaTime/ buildUpBeforeExplosion;

            //Color flicker on death
            if(Random.Range(0f, 1f) > 0.5f)
                bodyRenderer.material.SetColor("_EmissionColor", emissionColor * minMaxEmissionOnDeath.x);
            else
                bodyRenderer.material.SetColor("_EmissionColor", emissionColor * minMaxEmissionOnDeath.y);

            yield return null;
        }
        //yield return new WaitForSeconds(buildUpBeforeExplosion);

        GameObject hitParticle = Instantiate(explosionFX, transform.position, Quaternion.identity);
        Destroy(hitParticle, 1f);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        int i = 0;
        while (i < hitColliders.Length)
        {
            IHitable potentialHitableObject = hitColliders[i].GetComponent<IHitable>();
            if (potentialHitableObject != null)
            {
                potentialHitableObject.OnHit(null, (hitColliders[i].transform.position - transform.position).normalized, null, explosionDamage, DamageSource.RedBarrelExplosion, bumpValues);
            }
            i++;
        }
        FeedbackManager.SendFeedback("event.RedBarrelExplosion", this);
        SoundManager.PlaySound("RedBarrelExplosion", transform.position, transform);
        base.Die();
    }

    
}
