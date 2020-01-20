using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : PawnController
{
    [Space(2)]
    [Separator("Boss Variables")]
    public Renderer[] renderers;
    public Color normalColor = Color.blue;
    public Color attackingColor = Color.red;
   

    [Space(2)]
    [Header("Attack")]
    public Vector2 minMaxAttackSpeed = new Vector2(7,15);
    public AnimationCurve attackSpeedVariation;
    public float maxRotationSpeed = 20; // How many angle it can rotates in one second
    public float BumpOtherDistanceMod = 0.5f;
    public float BumpOtherDurationMod = 0.2f;
    public float BumpOtherRestDurationMod = 0.3f;
   
    }
    

