using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLaserBarrage : EnemyBehaviour
{
    [Space(2)]
    [Separator("Big Laser Attack Variables")]
    public bool isFiring =  true;
    public GameObject laserLineGO;
    private GameObject laserLineInstance;
    private LineRenderer laserLine;
    public float laserThickness = 3f;
    private float laserLength;

    public override void AttackingState()
    {
        isFiring = true;
        if (isFiring)
        {
            // Detection Part
            float internal_thickness = laserThickness; //<-- Desired thickness here.
            Vector3 internal_origin = transform.position + transform.forward * 0.5f;
            Vector3 internal_direction = transform.TransformDirection(Vector3.forward);
            RaycastHit hit;
            if (Physics.SphereCast(internal_origin, internal_thickness, internal_direction, out hit))
            {
                Debug.Log("hit " + hit.collider.name);
                laserLength = (hit.point - internal_origin).magnitude;
            }

            // Visual Part
            if (laserLineInstance == null)
            {
                laserLineInstance = Instantiate(laserLineGO, transform);
                laserLine = laserLineInstance.GetComponent<LineRenderer>();
            }
            laserLineInstance.transform.position = transform.position + new Vector3(transform.forward.x * 0.5f, 0.5f, transform.forward.z * 0.5f);
            laserLine.SetPosition(1, new Vector3(laserLine.GetPosition(1).x, laserLine.GetPosition(1).y, laserLength));
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
