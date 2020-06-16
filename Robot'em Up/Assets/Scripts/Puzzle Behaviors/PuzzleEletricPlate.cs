﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Analytics;

public class PuzzleEletricPlate : PuzzleActivable
{
    [ReadOnly]
    public float waitTimeBeforeNextDamage;
    [ReadOnly]
    public List<PawnController> pawnTrapped;
    public List<GameObject> IdleFx;
    public List<ParticleSystem> FXS;
    public float speedModifier = 0.5f;
    List<Renderer> ledList = new List<Renderer>();
    public Transform plateHolder;
    int ledNB;

    // Update is called once per frame
    void Awake()
    {
        IdleFx = new List<GameObject>();
        pawnTrapped.Clear();

        //GET ALL LEDS
        for (int i = 0; i < plateHolder.childCount; i++)
        {
            print(i);
            if(plateHolder.GetChild(i).name == "PlatePart_Side")
            {
                Renderer[] i_leds = plateHolder.GetChild(i).GetChild(0).GetChild(0).GetComponentsInChildren<Renderer>();
                for (int j = 0; j < i_leds.Length; j++)
                {
                    ledList.Add(i_leds[j]);
                }
            }
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < pawnTrapped.Count; i++)
        {
            PawnController item = pawnTrapped[i];
            if (item.GetHealth() < 1)
            {
                pawnTrapped.Remove(item);
            }
        }
        waitTimeBeforeNextDamage -= Time.deltaTime;

        if (waitTimeBeforeNextDamage < 0 && !isActivated && !shutDown)
        {
            waitTimeBeforeNextDamage = puzzleData.timeCheckingDamageEletricPlate;
            foreach (PawnController item in pawnTrapped)
            {
                if (item.GetComponent<EnemyBehaviour>())
                {
                    item.Damage(puzzleData.DamageEletricPlateEnnemies);

                }
                else
                {
                    item.Damage(puzzleData.DamageEletricPlate);
                }
				Analytics.CustomEvent("ElectricalPlateDamage", new Dictionary<string, object> { { "Zone", GameManager.GetCurrentZoneName() }, });
				item.AddSpeedModifier(new SpeedCoef(speedModifier, puzzleData.timeCheckingDamageEletricPlate, SpeedMultiplierReason.Freeze, false));

				FeedbackManager.SendFeedback("event.PuzzleElectricPlateDamage", item);
            }
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.GetComponent<PawnController>())
        {
            
            PawnController _pawn = _other.gameObject.GetComponent<PawnController>();
            if (_pawn.ignoreEletricPlates == false & !shutDown)
            {
                pawnTrapped.Add(_pawn);
            }

            if (!isActivated &!shutDown)
            {
                _pawn.AddSpeedModifier(new SpeedCoef(0.5f, puzzleData.timeCheckingDamageEletricPlate, SpeedMultiplierReason.Freeze, false));
            }
        }

    }


    private void OnTriggerExit(Collider _other)
    {
        if (_other.gameObject.GetComponent<PawnController>())
        {
            PawnController _pawn = _other.gameObject.GetComponent<PawnController>();
            pawnTrapped.Remove(_pawn);
        }

    }

    void OnDisable()
    {
        pawnTrapped.Clear();
    }

    override public void Activate()
    {
        if (!shutDown)
        {
            StopAllCoroutines();
            isActivated = true;

            //UpdateLights();

            foreach (ParticleSystem ps in FXS)
            {
                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = false;
            }
            for (int i = 0; i < ledList.Count; i++)
            {
                ledList[i].material.SetColor("_EmissionColor", Color.cyan);
            }
        }

    }

    override public void Desactivate()
    {
        if (!shutDown)
        {
            bool i_checkAllConditionsCustom = true;

            foreach (var item in puzzleActivators)
            {
                if (item.isActivated)
                {
                    i_checkAllConditionsCustom = false;
                }
            }

            if (i_checkAllConditionsCustom)
            {
                StartCoroutine(GettingEletrified());
            }
        }
    }

    public IEnumerator GettingEletrified ()
    {
        for (int i = 0; i < ledList.Count; i++)
        {
            ledList[i].material.SetColor("_EmissionColor", Color.yellow);
        }
        yield return new WaitForSeconds(puzzleData.timeOrangePressurePlate);

        for (int i = 0; i < ledList.Count; i++)
        {
            ledList[i].material.SetColor("_EmissionColor", Color.red);
        }
        isActivated = false;
        //UpdateLights();

        foreach (ParticleSystem ps in FXS)
        {
            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = true;

        }
        FeedbackManager.SendFeedback("event.PuzzleElectricPlateActivation", this);
    }

    public override void CustomShutDown()
    {
        foreach (ParticleSystem ps in FXS)
        {
            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = false;
        }
    }
}
