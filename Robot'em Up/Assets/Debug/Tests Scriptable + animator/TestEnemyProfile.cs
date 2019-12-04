using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Profile.asset", menuName = "Enemy Profile", order = 100)]
public class TestEnemyProfile : ScriptableObject
{
    public float speed;
    public Color color;
    public AnimationCurve acceleration;
}
