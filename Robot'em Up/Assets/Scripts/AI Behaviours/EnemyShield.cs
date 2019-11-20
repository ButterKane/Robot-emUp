using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShield : EnemyBehaviour
{
    [Space(2)]
    [Separator("Shield Variables")]
    public GameObject Shield;

    public override void EnterPreparingAttackState()
    {
        anticipationTime = maxAnticipationTime;
    }
    public override void PreparingAttackState()
    {
        anticipationTime -= Time.deltaTime;
        if (anticipationTime <= 0)
        {
            ChangingState(EnemyState.Attacking);
        }
    }

    public override void AttackingState()
    {
        anticipationTime -= Time.deltaTime;
        if (anticipationTime <= 0)
        {
            ChangingState(EnemyState.Idle);
        }
    }
}
