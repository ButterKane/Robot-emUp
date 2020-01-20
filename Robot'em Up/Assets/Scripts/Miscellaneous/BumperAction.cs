using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperAction : MonoBehaviour
{
    [Header("Bump")]
    public float maxGettingUpDuration = 0.6f;
    public AnimationCurve bumpDistanceCurve;
    public float bumpDistance;
    public float bumpDuration;
    public float restDuration;
    public float gettingUpDuration;
    public float randomDistanceMod;
    public float randomDurationMod;
    public float randomRestDurationMod;

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.tag == "Enemy")
        {
            EnemyBehaviour i_enemy = _other.gameObject.GetComponent<EnemyBehaviour>();
            if (i_enemy)
            {
                Vector3 i_bumpDirection = SwissArmyKnife.GetFlattedDownDirection( i_enemy.transform.position - transform.position);
                i_enemy.BumpMe(bumpDistance, bumpDuration, restDuration, i_bumpDirection, randomDistanceMod, randomDurationMod, randomRestDurationMod);
            }
        }
        if (_other.gameObject.tag == " Player")
        {
            PawnController i_player = _other.gameObject.GetComponent<PawnController>();
            if (i_player)
            {
                Vector3 bumpDirection = SwissArmyKnife.GetFlattedDownDirection(i_player.transform.position - transform.position);
                i_player.BumpMe(bumpDistance, bumpDuration, restDuration, bumpDirection, randomDistanceMod, randomDurationMod, randomRestDurationMod);
            }
        }
    }
}
