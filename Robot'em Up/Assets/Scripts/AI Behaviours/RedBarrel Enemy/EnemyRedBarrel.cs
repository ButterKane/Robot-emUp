using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRedBarrel : EnemyBehaviour
{
    [Space(2)]
    [Separator("Red Barrel Death variables")]
    public bool willExplodeWhenKilled;
    [ReadOnly] public bool isExplosionSafe;
    public string buildUpExplosionFX = "event.RedBarrelEnemyDeathPart1";
    public string buildUpSafeExplosionFX = "event.RedBarrelEnemyDeathPart1Bis";
    public string explosionFX = "event.RedBarrelEnemyDeathPart2";
    public string safeExplosionFX = "event.RedBarrelEnemyDeathPart2Bis";
    public float explosionRadius = 3f;
    public int explosionDamage = 10;
    public float buildUpBeforeExplosion = 0.5f;
    public float explosionFXScale = 3;
    [HideInInspector] public bool willExplode = true;
    public LayerMask layersToCheckForExplosion;

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
        eventOnBeingHit = "event.EnemyRedBarrelHit";
        eventOnDeath = "event.EnemyRedBarrelDeathPart2";
        bumpValues = new Vector3(bumpDistanceMod, bumpDurationMod, bumpRestDurationMod);
        explosionRadiusTransform.localScale = new Vector3(explosionRadius * 2, explosionRadius * 2, explosionRadius * 2);
        Explosion_C = null;
    }

    public override void PreparingAttackState()
    {
        
    }

    public override void EnterPreparingAttackState()
    {
        navMeshAgent.enabled = false;
        bodyRenderer.material = materialOnExplosion;
        LaunchExplosion();
        currentAnticipationTime = maxAnticipationTime;
    }
    

    public void LaunchExplosion()
    {
        if (GetHealth() <= 0)
        {
            if (willExplodeWhenKilled) { isExplosionSafe = true; }
            else { base.Kill(); return; }
        }
        else { isExplosionSafe = false; }

        if (Explosion_C == null)
        {
            Explosion_C = ExplosionSequence_C(isExplosionSafe);
        }
        else
        {
            StopCoroutine(Explosion_C);
            Explosion_C = ExplosionSequence_C(isExplosionSafe);
        }
        ChangePawnState("RedBarrelAnticipating", Explosion_C, CancelExplosionSequence_C());
    }

    public void Explode()
    {
        GameObject explosionFXInstance = FeedbackManager.SendFeedback(explosionFX, this).GetVFX();
        explosionFXInstance.transform.localScale = new Vector3(explosionFXScale, explosionFXScale, explosionFXScale);

        Collider[] i_hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        int i = 0;
        while (i < i_hitColliders.Length)
        {
            IHitable potentialHitableObject = i_hitColliders[i].GetComponent<IHitable>();
            if (potentialHitableObject != null)
            {
                potentialHitableObject.OnHit(null, (i_hitColliders[i].transform.position - transform.position).normalized, this, explosionDamage, DamageSource.RedBarrelExplosion, bumpValues);
            }
            i++;
        }
    }

    public void SafeExplode() // explodes, but only touches enemies
    {
        GameObject explosionFXInstance = FeedbackManager.SendFeedback(safeExplosionFX, this).GetVFX();
        explosionFXInstance.transform.localScale = new Vector3(explosionFXScale, explosionFXScale, explosionFXScale);

        Collider[] i_hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, layersToCheckForExplosion) ;
        int i = 0;
        while (i < i_hitColliders.Length)
        {
            IHitable potentialHitableObject = i_hitColliders[i].GetComponent<IHitable>();
            if (potentialHitableObject != null && i_hitColliders[i].gameObject.tag == "Enemy") 
            {
                potentialHitableObject.OnHit(null, (i_hitColliders[i].transform.position - transform.position).normalized, null, explosionDamage, DamageSource.RedBarrelExplosion, bumpValues);
            }
            i++;
        }
    }

    private IEnumerator ExplosionSequence_C(bool _isSafeExplosion)
    {
		Debug.Log("Starting explosion sequence");
        animator.SetTrigger("DeathTrigger");
        if (_isSafeExplosion) { FeedbackManager.SendFeedback(buildUpSafeExplosionFX, this); }
        else { FeedbackManager.SendFeedback(buildUpExplosionFX, this); }
        
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
            explosionRadiusTransform.gameObject.SetActive(false);
            explosionGrowingRenderer.localScale = Vector3.zero;
            Explosion_C = null;
            yield break;
        }
        else
        {
            if (_isSafeExplosion)
            {
                SafeExplode();
            }
            else
            {
                Explode();
            }
            Kill();
        }
    }

	private IEnumerator CancelExplosionSequence_C ()
	{
        StopCoroutine(Explosion_C);
		Explosion_C = null;
		willExplode = false;
		explosionRadiusTransform.gameObject.SetActive(false);
		explosionGrowingRenderer.localScale = Vector3.zero;
		yield return null;
	}

    public override void HeavyPushAction()
    {
        currentAnticipationTime = 0;
        ChangeState(EnemyState.Idle);
    }
}
