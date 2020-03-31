using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PawnMovementValues", menuName = "GlobalDatas/PawnMovementValues", order = 1)]
public class PawnMovementValues : ScriptableObject
{
    public AnimationCurve accelerationCurve;
    [Tooltip("Minimum required speed to go to walking state")] public float minWalkSpeed = 0.1f;
    public float moveSpeed = 15;
    public float acceleration = 200;
    public float movingDrag = .4f;
    public float idleDrag = .4f;
    public float onGroundGravityMultiplier;
    public float deadzone = 0.2f;
    [Range(0.01f, 1f)] public float turnSpeed = .25f;
}
