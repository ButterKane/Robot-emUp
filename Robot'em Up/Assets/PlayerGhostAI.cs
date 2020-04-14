using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGhostAI : MonoBehaviour
{
    public float actionCooldown = 5;
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
            passController.Shoot();
            currentCooldown = actionCooldown;
        } else
        {
            passController.Aim();
            passController.SetLookDirection(transform.right);
            currentCooldown -= Time.deltaTime;
        }
    }
}
