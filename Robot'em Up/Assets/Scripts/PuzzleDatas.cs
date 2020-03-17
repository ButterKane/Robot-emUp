using System.Collections;
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
    [Range(0, 100)]
    public int DamageEletricPlate;
    [Range(0, 100)]
    public int DamageEletricPlateEnnemies;
    [Range(0.01f, 0.5f)]
    public float timeCheckingDamageEletricPlate;
    [Range(0f, 1f)]
    public float timeOrangePressurePlate;

    [Header("FX")]
	public GameObject charging;
    public GameObject linked;
    public GameObject linking;
    public GameObject linkEnd;
    public GameObject linkStop;
    public GameObject doorOpening;
    public GameObject electricPlateActivate;
    public GameObject electricPlateDamage;

    [Header("Colors")]
    public Color repeaterActivate;
    public Color repeaterDesactivate;

    [Header("Materials")]
    public Material m_forcefield_Active;
    public Material m_forcefield_Desactivated;
    public Material m_forcefieldPlayers_Active;
    public Material m_forcefieldPlayers_Desactivated;
    public Material m_forcefield_Flipper_Active;
    public Material m_forcefield_Flipper_Desactivated;
    public Material m_switchActivate;
    public Material m_switchDesactivate;
    public Material m_puzzleElectreticPlate;
    public Material m_puzzleElectreticPlate_Orange;
    public Material m_puzzleElectreticPlate_Activated;


}
