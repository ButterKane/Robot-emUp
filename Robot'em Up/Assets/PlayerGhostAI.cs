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
    private float curvedCooldown;
    private Vector3 OriginalPosition;
    private Vector3 ThrustX;
    private Vector3 ThrustZ;
    private int StepMoving;
    private bool jumpCooldown;

    private DashController dashController;
    private DunkController dunkController;
    private PassController passController;
    private PawnController pawnController;

    [ConditionalField(nameof(ghostType), false, GhostType.Passing, GhostType.Dunk, GhostType.CurvedPassing)]  public PawnController passTarget;
    [ConditionalField(nameof(ghostType), false, GhostType.Dunk)] public DunkType myDunkType;

    private void Awake ()
    {
        dashController = GetComponent<DashController>();
        passController = GetComponent<PassController>();
        dunkController = GetComponent<DunkController>();
        pawnController = GetComponent<PawnController>();
        curvedCooldown = 0;
        StepMoving = 0;
        OriginalPosition = transform.position;
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

                switch (StepMoving)
                {
                    case 0:
                        CurrentDirection = new Vector3(1, 0, 0);
                        break;
                    case 1:
                        CurrentDirection = new Vector3(1, 0, 1);
                        break;
                    case 2:
                        CurrentDirection = new Vector3(-1, 0, 0);
                        break;
                    case 3:
                        CurrentDirection = new Vector3(-1, 0, -1);
                        break;
                }


                if (currentCooldown <= 0)
                {
                    StepMoving++;
                    if (StepMoving>3)
                        {
                                StepMoving = 0;
                    }
                    currentCooldown = actionCooldown;
                }
                else
                {
                    currentCooldown -= Time.deltaTime;
                }

                pawnController.moveInput = CurrentDirection * SpeedGhost;
                pawnController.lookInput = CurrentDirection;
                pawnController.UpdateAnimatorBlendTree();


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
                pawnController.canMove = false;
                if (currentCooldown <= 0)
                {
                    transform.LookAt(passTarget.transform.position);
                    passController.Shoot();
                    currentCooldown = actionCooldown;
                    curvedCooldown = 0;
                }
                else
                {
                    passController.Aim();
                    curvedCooldown += Time.deltaTime;
                    passController.SetLookDirection(new Vector3(0.2f, 0, - curvedCooldown * 0.2f));
                    // passController.SetLookDirection(Vector3.MoveTowards(transform.position,passTarget.transform.position, 100));
                    currentCooldown -= Time.deltaTime;
                }
                break;
            case GhostType.Dunk:

                pawnController.canMove = false;
                if (currentCooldown <= 0.5f && myDunkType == DunkType.Dunking && jumpCooldown && !passController.CanShoot())
                {
                    dunkController.ForceDunk();
                    jumpCooldown = false;
                }
                if (currentCooldown <= 0 && passTarget != null)
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
