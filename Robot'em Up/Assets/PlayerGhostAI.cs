using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGhostAI : MonoBehaviour
{
    public float dashCooldown = 5;
    private float currentCooldown;

    private DashController dashController;
    private DunkController dunkController;
    private PassController passController;
    private PawnController pawnController;

    public PawnController passTarget;

    private void Awake ()
    {
        dashController = GetComponent<DashController>();
        passController = GetComponent<PassController>();
        dunkController = GetComponent<DunkController>();
        pawnController = GetComponent<PawnController>();


        if (passController) { passController.SetTargetedPawn(passTarget); }
    }
    void Update()
    {
        if (currentCooldown <= 0)
        {
            transform.LookAt(passTarget.transform.position);
            if (passController.CanShoot())
            {
                StartCoroutine(DunkAction());
            }
            //dunkController.ForceDunk();
            //dashController.Dash(transform.forward);
            currentCooldown = dashCooldown;
        } else
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    IEnumerator DunkAction()
    {
        passTarget.GetComponent<DunkController>().ForceDunk();
        yield return new WaitForSeconds(0.25f);
        passController.Shoot();
        passController.SetLookDirection(passTarget.transform.position - transform.position);
    }
}
