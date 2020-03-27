using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMelee : EnemyBehaviour
{
    [Separator("Melee Variables")]
    public GameObject attackHitBoxPrefab;
    public Transform attackHitBoxCenterPoint;
    [Range(0, 1)] public float portionOfAnticipationWithFlickering = 0.2f;

    private MeshRenderer attackPreviewPlaneRenderer;
    private GameObject attackHitBoxInstance;
    private GameObject attackPreviewPlane;
    GameObject myAttackHitBox;

    private EnemyArmAttack armScript;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        eventOnBeingHit = "event.EnemyMeleeHit";
        eventOnDeath = "event.EnemyDeath";
        armScript = GetComponentInChildren<EnemyArmAttack>();

        InititateMeleeHitBox();
    }

    public override void EnterPreparingAttackState()
    {
        //ChangePawnState("MeleeEnemyAnticipating", StartAttackState_C(), StopAttackState_C());
        base.EnterPreparingAttackState();
        ActivateMeleeHitBox();
    }

    public void InititateMeleeHitBox()
    {
        if (attackHitBoxInstance == null && enemyType == EnemyTypes.Melee)
        {
            attackHitBoxInstance = Instantiate(attackHitBoxPrefab, new Vector3(attackHitBoxCenterPoint.position.x, attackHitBoxCenterPoint.position.y + attackHitBoxCenterPoint.localScale.y / 2, attackHitBoxCenterPoint.position.z), Quaternion.identity, transform);
            attackHitBoxInstance.GetComponent<EnemyArmAttack>().attackDamage = damage;
            attackHitBoxInstance.GetComponent<EnemyArmAttack>().spawnParent = this;
            attackPreviewPlane = attackHitBoxInstance.GetComponent<EnemyArmAttack>().highlightPlane;
            attackPreviewPlaneRenderer = attackPreviewPlane.GetComponent<MeshRenderer>();
            attackHitBoxInstance.SetActive(false);
        }
    }

    public void ActivateMeleeHitBox()
    {
        attackHitBoxInstance.SetActive(true);
    }

    public void MeleeAttackPreview(float _anticipationTime)
    {
        if (attackPreviewPlane != null)
        {
            // Make attack zone appear progressively
            if (_anticipationTime > portionOfAnticipationWithFlickering * maxAnticipationTime)
            {
                attackPreviewPlane.transform.localScale = Vector3.one * (1 - ((_anticipationTime - (portionOfAnticipationWithFlickering * maxAnticipationTime)) / (maxAnticipationTime - (maxAnticipationTime * portionOfAnticipationWithFlickering))));
            }
            // If max size is reached, flicker the color
            else
            {
                attackPreviewPlane.transform.localScale = Vector3.one;
                attackPreviewPlaneRenderer.enabled = !attackPreviewPlaneRenderer.enabled;
            }
        }

        if (_anticipationTime <= 0)
        {
            if (attackPreviewPlaneRenderer != null) { attackPreviewPlaneRenderer.enabled = true; }
        }
    }

    public override void PreparingAttackState()
    {
        base.PreparingAttackState();
        MeleeAttackPreview(currentAnticipationTime);
    }

    public void ActivateAttackHitBox()
    {
        FeedbackManager.SendFeedback("event.EnemyMeleeAttack", this);
        if (attackHitBoxInstance != null)
        {
            attackHitBoxInstance.GetComponent<EnemyArmAttack>().ToggleArmCollider(true);
        }
    }

    public override void DestroySpawnedAttackUtilities()
    {
        if (attackHitBoxInstance != null)
        {
            attackHitBoxInstance.SetActive(false);
        }
    }

    public IEnumerator StartAttackState_C()
    {
        Debug.Log("starting attack");
        base.EnterPreparingAttackState();
        attackPreviewPlane = null;
        InititateMeleeHitBox();
        yield return null;
    }

    public IEnumerator StopAttackState_C()
    {
        Debug.Log("stopping attack");
        DestroySpawnedAttackUtilities();
        yield return null;
    }

    public override void HeavyPushAction()
    {
        cooldownDuration = cooldownAfterAttackTime;
        currentAnticipationTime = 0;
        animator.ResetTrigger("AnticipateAttackTrigger");
        animator.ResetTrigger("AttackTrigger");
        ChangeState(EnemyState.Idle);
    }
}
