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
    public float bumpHeight;
    public float gettingUpDuration;

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.tag == "Enemy")
        {
            EnemyBehaviour i_enemy = _other.gameObject.GetComponent<EnemyBehaviour>();
            if (i_enemy)
            {
                Vector3 i_bumpDirection = SwissArmyKnife.GetFlattedDownDirection( i_enemy.transform.position - transform.position);
                i_enemy.BumpMe(i_bumpDirection, BumpForce.Force3);
            }
        }
        if (_other.gameObject.tag == " Player")
        {
            PawnController i_player = _other.gameObject.GetComponent<PawnController>();
            if (i_player)
            {
                Vector3 bumpDirection = SwissArmyKnife.GetFlattedDownDirection(i_player.transform.position - transform.position);
                i_player.BumpMe(bumpDirection, BumpForce.Force3);
            }
        }
    }
}
