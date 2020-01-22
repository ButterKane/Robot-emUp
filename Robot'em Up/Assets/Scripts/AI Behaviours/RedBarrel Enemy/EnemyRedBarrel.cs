using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRedBarrel : EnemyBehaviour
{
    [Space(2)]
    [Separator("Red Barrel Death variables")]
    public string buildUpExplosionFX = "event.RedBarrelEnemyDeathPart1";
    public string explosionFX = "event.RedBarrelEnemyDeathPart2";
    public float explosionRadius = 3f;
    public int explosionDamage = 10;
    public float buildUpBeforeExplosion = 0.5f;
    public float explosionFXScale = 3;
    [HideInInspector] public bool willExplode = true;

    [Separator("Explosion Bump variables")]
    public float bumpDistanceMod = 1.5f;
    public float bumpDurationMod = 0.7f;
    public float bumpRestDurationMod = 0.2f;
    private Vector3 bumpValues;

    public Renderer explosionRadiusRenderer;
    public Transform explosionGrowingRenderer;
    public Transform explosionRadiusTransform;
    public Renderer bodyRenderer;
    public Material materialOnExplosion;
    public Color emissionColor;
    public Vector2 minMaxEmissionOnDeath;

    private IEnumerator Explosion_C;

    new void Start()
    {
        base.Start();
        bumpValues = new Vector3(bumpDistanceMod, bumpDurationMod, bumpRestDurationMod);
        explosionRadiusTransform.localScale = new Vector3(explosionRadius * 2, explosionRadius * 2, explosionRadius * 2);
        Explosion_C = null;
    }

    public override void PreparingAttackState()
    {
        ChangeState(EnemyState.Dying);
        bodyRenderer.material = materialOnExplosion;
    }

    public override void Kill()
    {
        if (Explosion_C == null)
        {
            Explosion_C = ExplosionSequence_C();
            StartCoroutine(Explosion_C);
        }
    }

    private IEnumerator ExplosionSequence_C()
    {
        animator.SetTrigger("DeathTrigger");
        FeedbackManager.SendFeedback(buildUpExplosionFX, this);
        willExplode = true;

        //hitParticle.transform.localScale = 3f;
        explosionRadiusTransform.gameObject.SetActive(true);
        float i_time = 0;
        while (i_time < 1 && willExplode)
        {
            explosionGrowingRenderer.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, i_time);
            i_time += Time.deltaTime / buildUpBeforeExplosion;

            //Color flicker on death
            if (Random.Range(0f, 1f) > 0.5f)
                bodyRenderer.material.SetColor("_EmissionColor", emissionColor * minMaxEmissionOnDeath.x);
            else
                bodyRenderer.material.SetColor("_EmissionColor", emissionColor * minMaxEmissionOnDeath.y);

            yield return null;
        }

        if (!willExplode)
        {
            Debug.Log("On a desactivé willExplode");
            explosionRadiusTransform.gameObject.SetActive(false);
            explosionGrowingRenderer.localScale = Vector3.zero;
            Explosion_C = null;
            yield break;
        }
        else
        {
            //yield return new WaitForSeconds(buildUpBeforeExplosion);
            FeedbackManager.SendFeedback(explosionFX, this);

            Collider[] i_hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            int i = 0;
            while (i < i_hitColliders.Length)
            {
                IHitable potentialHitableObject = i_hitColliders[i].GetComponent<IHitable>();
                if (potentialHitableObject != null)
                {
                    potentialHitableObject.OnHit(null, (i_hitColliders[i].transform.position - transform.position).normalized, null, explosionDamage, DamageSource.RedBarrelExplosion, bumpValues);
                }
                i++;
            }

            base.Kill();
        }
    }
}
