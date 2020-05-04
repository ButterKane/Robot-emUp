using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PlayerGhostAI : MonoBehaviour
{


    public enum GhostType { Moving, Dashing, Passing, CurvedPassing, Dunk };
    public enum DunkType { Passing, Dunking };

    public GhostType ghostType;

    [ConditionalField(nameof(ghostType), false, GhostType.Moving, GhostType.Dashing)] public float SpeedGhost;
    [ConditionalField(nameof(ghostType), false, GhostType.Moving, GhostType.Dashing)] public Vector3 Direction1;
    [ConditionalField(nameof(ghostType), false, GhostType.Moving, GhostType.Dashing)] public Vector3 Direction2;
    private Vector3 CurrentDirection;


    public float actionCooldown = 5;
    private float currentCooldown;
    private bool jumpCooldown;

    private DashController dashController;
    private DunkController dunkController;
    private PassController passController;
    private PawnController pawnController;

    [ConditionalField(nameof(ghostType), false, GhostType.Passing, GhostType.Dunk)]  public PawnController passTarget;
    [ConditionalField(nameof(ghostType), false, GhostType.Dunk)] public DunkType myDunkType;

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
        pawnController.animator.SetFloat("ForwardBlend", pawnController.GetCurrentSpeed() / pawnController.pawnMovementValues.moveSpeed);
        pawnController.animator.SetFloat("SideBlend", 0);
        switch (ghostType)
        {
            case GhostType.Moving:
                pawnController.canMove = true;
                pawnController.moveInput = CurrentDirection * SpeedGhost;
                pawnController.lookInput = CurrentDirection;
                pawnController.UpdateAnimatorBlendTree();
                if (currentCooldown <= 0)
                {
                    currentCooldown = actionCooldown;
                    if (CurrentDirection == Direction1)
                    {
                        CurrentDirection = Direction2;
                    }
                    else
                    {
                        CurrentDirection = Direction1;
                    }
                }
                else
                {
                    currentCooldown -= Time.deltaTime;
                }

                break;
            case GhostType.Dashing:

                pawnController.canMove = true;
                pawnController.lookInput = CurrentDirection;
                pawnController.moveInput = CurrentDirection * SpeedGhost;
                pawnController.UpdateAnimatorBlendTree();

                if (currentCooldown <= 0)
                {
                    dashController.RecoverAllStackAmount();
                    dashController.Dash(CurrentDirection);
                    pawnController.lookInput = CurrentDirection;
                    currentCooldown = actionCooldown;
                    if (CurrentDirection == Direction1)
                    {
                        CurrentDirection = Direction2;
                    }
                    else
                    {
                        CurrentDirection = Direction1;
                    }
                }
                else
                {
                    currentCooldown -= Time.deltaTime;
                }
                break;
            case GhostType.Passing:
                pawnController.canMove = false;
                if (currentCooldown <= 0)
                {
                    transform.LookAt(passTarget.transform.position);
                    passController.Shoot();
                    currentCooldown = actionCooldown;
                }
                else
                {
                    passController.Aim();
                   // passController.SetLookDirection(Vector3.MoveTowards(transform.position,passTarget.transform.position, 100));
                    currentCooldown -= Time.deltaTime;
                }
                break;
            case GhostType.CurvedPassing:
                break;
            case GhostType.Dunk:

                pawnController.canMove = false;
                if (currentCooldown <= 0.5f && myDunkType == DunkType.Dunking && jumpCooldown && !passController.CanShoot())
                {
                    dunkController.ForceDunk();
                    jumpCooldown = false;
                }
                if (currentCooldown <= 0)
                {
                    transform.LookAt(passTarget.transform.position);
                    passController.Shoot();
                    currentCooldown = actionCooldown;
                    jumpCooldown = true;
                }
                else
                {
                    passController.Aim();
                    // passController.SetLookDirection(Vector3.MoveTowards(transform.position,passTarget.transform.position, 100));
                    currentCooldown -= Time.deltaTime;
                }

                break;
        }
    }
}
