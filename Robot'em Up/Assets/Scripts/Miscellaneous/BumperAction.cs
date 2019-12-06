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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            EnemyBehaviour enemy = other.gameObject.GetComponent<EnemyBehaviour>();
            if (enemy)
            {
                Vector3 bumpDirection = SwissArmyKnife.GetFlattedDownDirection( enemy.transform.position - transform.position);
                enemy.BumpMe(bumpDistance, bumpDuration, restDuration, bumpDirection, randomDistanceMod, randomDurationMod, randomRestDurationMod);
            }
        }
        if (other.gameObject.tag == " Player")
        {
            PawnController player = other.gameObject.GetComponent<PawnController>();
            if (player)
            {
                Vector3 bumpDirection = SwissArmyKnife.GetFlattedDownDirection(player.transform.position - transform.position);
                player.BumpMe(bumpDistance, bumpDuration, restDuration, bumpDirection, randomDistanceMod, randomDurationMod, randomRestDurationMod);
            }
        }
    }
}
