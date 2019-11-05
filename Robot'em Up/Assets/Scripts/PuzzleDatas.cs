﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleDatas", menuName = "GameDatas/PuzzleDatas", order = 1)]
public class PuzzleDatas : ScriptableObject
{
	[Header("Global settings")]
    public bool showTuto;
    [Range(0, 1)]
    public float nbMomentumChargedByCharger;
    [Range(0, 1)]
    public float nbMomentumNeededToLink;
    [Range(0, 1)]
    public float nbMomentumLooseWhenLink;
    [Range(0, 20)]
    public float nbSecondsLinkMaintained;
    [Range(0, 100)]
    public int DamageEletricPlate;
    [Range(0.01f, 0.5f)]
    public float timeCheckingDamageEletricPlate;

    [Header("FX")]
	public GameObject Charging;
    public GameObject Linked;
    public GameObject Linking;
    public GameObject LinkEnd;
    public GameObject LinkStop;
    public GameObject DoorOpening;
    public GameObject ElectricPlateActivate;
    public GameObject ElectricPlateDamage;

    [Header("Colors")]
    public Color RepeaterActivate;
    public Color RepeaterDesactivate;

    [Header("Materials")]
    public Material M_Forcefield_Active;
    public Material M_Forcefield_Desactivated;
    public Material M_ForcefieldPlayers_Active;
    public Material M_ForcefieldPlayers_Desactivated;
    public Material M_SwitchActivate;
    public Material M_SwitchDesactivate;
    public Material M_PuzzleElectreticPlate;
    public Material M_PuzzleElectreticPlate_Activated;


}
