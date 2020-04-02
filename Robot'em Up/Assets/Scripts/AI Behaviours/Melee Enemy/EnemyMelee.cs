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

    #region Public methods
    public void ActivateAttackHitBox()
    {
        FeedbackManager.SendFeedback("event.EnemyMeleeAttack", this);
        if (attackHitBoxInstance != null)
        {
            attackHitBoxInstance.GetComponent<EnemyArmAttack>().ToggleArmCollider(true);
        }
    }
    #endregion
    #region Private methods
    private void ActivateMeleeBoxInstance()
    {
        attackHitBoxInstance.SetActive(true);
    }

    private void MeleeAttackPreview(float _anticipationTime)
    {
        if (attackPreviewPlane != null)
        {
            // Make attack zone appear progressively
            if (_anticipationTime > portionOfAnticipationWithFlickering * attackValues.maxAnticipationTime)
            {
                attackPreviewPlane.transform.localScale = Vector3.one * (1 - ((_anticipationTime - (portionOfAnticipationWithFlickering * attackValues.maxAnticipationTime)) / (attackValues.maxAnticipationTime - (attackValues.maxAnticipationTime * portionOfAnticipationWithFlickering))));
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

    private void InititateMeleeHitBox()
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
    #endregion
    #region Overriden methods
    public override void EnterPreparingAttackState()
    {
        //ChangePawnState("MeleeEnemyAnticipating", StartAttackState_C(), StopAttackState_C());
        base.EnterPreparingAttackState();
        ActivateMeleeBoxInstance();
    }

    public override void PreparingAttackState()
    {
        base.PreparingAttackState();
        MeleeAttackPreview(currentAnticipationTime);
    }

    public override void DestroySpawnedAttackUtilities()
    {
        // well, we don't destroy anything here, but this is an override method so teh name isn't contractual
        if (attackHitBoxInstance != null)
        {
            attackHitBoxInstance.GetComponent<EnemyArmAttack>().ToggleArmCollider(false);
            attackHitBoxInstance.SetActive(false);
        }
    }

    public override void HeavyPushAction()
    {
        cooldownDuration = attackValues.cooldownAfterAttackTime;
        currentAnticipationTime = 0;
        animator.ResetTrigger("AnticipateAttackTrigger");
        animator.ResetTrigger("AttackTrigger");
        ChangeState(EnemyState.Idle);
    }
    #endregion
    #region Coroutines
    private IEnumerator StartAttackState_C()
    {
        Debug.Log("starting attack");
        base.EnterPreparingAttackState();
        attackPreviewPlane = null;
        InititateMeleeHitBox(); // just in case the hit box got a problem, recreate it
        yield return null;
    }

    private IEnumerator StopAttackState_C()
    {
        Debug.Log("stopping attack");
        DestroySpawnedAttackUtilities();
        yield return null;
    }
    #endregion
}
