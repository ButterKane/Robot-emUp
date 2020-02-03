﻿using MyBox;
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
        armScript = GetComponentInChildren<EnemyArmAttack>();
    }

    public override void EnterPreparingAttackState()
    {
        base.EnterPreparingAttackState();
        attackPreviewPlane = null;
        InititateMeleeHitBox();
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
        }
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
        MeleeAttackPreview(anticipationTime);
    }

    public void ActivateAttackHitBox()
    {
        if (attackHitBoxInstance != null)
        {
            attackHitBoxInstance.GetComponent<EnemyArmAttack>().ToggleArmCollider(true);
        }
    }

    public override void DestroySpawnedAttackUtilities()
    {
        if (attackHitBoxInstance != null)
        {
            Destroy(attackHitBoxInstance);
            attackHitBoxInstance = null;
        }
    }
}
