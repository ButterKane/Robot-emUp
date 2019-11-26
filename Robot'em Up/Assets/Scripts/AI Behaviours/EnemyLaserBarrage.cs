using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLaserBarrage : EnemyBehaviour
{
    [Space(2)]
    [Separator("Big Laser Attack Variables")]
    public bool isFiring =  true;
    public GameObject LaserLineGO;
    private GameObject LaserLineInstance;
    private LineRenderer LaserLine;
    public float LaserThickness = 3f;
    private float LaserLength;

    public override void AttackingState()
    {
        isFiring = true;
        if (isFiring)
        {
            // Detection Part
            float thickness = LaserThickness; //<-- Desired thickness here.
            Vector3 origin = transform.position + transform.forward * 0.5f;
            Vector3 direction = transform.TransformDirection(Vector3.forward);
            RaycastHit hit;
            if (Physics.SphereCast(origin, thickness, direction, out hit))
            {
                Debug.Log("hit " + hit.collider.name);
                LaserLength = (hit.point - origin).magnitude;
            }

            // Visual Part
            if (LaserLineInstance == null)
            {
                LaserLineInstance = Instantiate(LaserLineGO, transform);
                LaserLine = LaserLineInstance.GetComponent<LineRenderer>();
            }
            LaserLineInstance.transform.position = transform.position + new Vector3(transform.forward.x * 0.5f, 0.5f, transform.forward.z * 0.5f);
            LaserLine.SetPosition(1, new Vector3(LaserLine.GetPosition(1).x, LaserLine.GetPosition(1).y, LaserLength));
        }
            
        //}
        //attackTimeProgression += Time.deltaTime / attackDuration;
        ////attackDuration -= Time.deltaTime;

        ////must stop ?
        //int attackRaycastMask = 1 << LayerMask.NameToLayer("Environment");
        //if (Physics.Raycast(_self.position, _self.forward, attackRaycastDistance, attackRaycastMask) && !mustCancelAttack)
        //{
        //    attackTimeProgression = whenToTriggerEndOfAttackAnim;
        //    mustCancelAttack = true;
        //}

        //if (!mustCancelAttack)
        //{
        //    Rb.MovePosition(Vector3.Lerp(attackInitialPosition, attackDestination, attackSpeedCurve.Evaluate(attackTimeProgression)));
        //}

        //if (attackTimeProgression >= 1)
        //{
        //    ChangingState(EnemyState.PauseAfterAttack);
        //}
        //else if (attackTimeProgression >= whenToTriggerEndOfAttackAnim && !endOfAttackTriggerLaunched)
        //{
        //    endOfAttackTriggerLaunched = true;
        //    Animator.SetTrigger("EndOfAttackTrigger");
        //}
    }

}
