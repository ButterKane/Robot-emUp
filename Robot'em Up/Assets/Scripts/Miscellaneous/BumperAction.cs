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
            EnemyBehaviour internal_enemy = _other.gameObject.GetComponent<EnemyBehaviour>();
            if (internal_enemy)
            {
                Vector3 internal_bumpDirection = SwissArmyKnife.GetFlattedDownDirection( internal_enemy.transform.position - transform.position);
                internal_enemy.BumpMe(bumpDistance, bumpDuration, restDuration, internal_bumpDirection, randomDistanceMod, randomDurationMod, randomRestDurationMod);
            }
        }
        if (_other.gameObject.tag == " Player")
        {
            PawnController internal_player = _other.gameObject.GetComponent<PawnController>();
            if (internal_player)
            {
                Vector3 bumpDirection = SwissArmyKnife.GetFlattedDownDirection(internal_player.transform.position - transform.position);
                internal_player.BumpMe(bumpDistance, bumpDuration, restDuration, bumpDirection, randomDistanceMod, randomDurationMod, randomRestDurationMod);
            }
        }
    }
}
