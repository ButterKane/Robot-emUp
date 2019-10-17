using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleDatas", menuName = "GameDatas/PuzzleDatas", order = 1)]
public class PuzzleDatas : ScriptableObject
{
	[Header("Global settings")]
    [Range(0, 1)]
    public float nbMomentumChargedByCharger;
    [Range(0, 1)]
    public float nbMomentumNeededToLink;
    [Range(0, 20)]
    public float nbSecondsLinkMaintained;

    [Header("FX")]
	public GameObject Charging;
    public GameObject Linked;
    public GameObject LinkEnd;
    public GameObject DoorOpening;
}
