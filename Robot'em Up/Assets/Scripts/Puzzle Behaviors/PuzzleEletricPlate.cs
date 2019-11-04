﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzleEletricPlate : PuzzleActivable
{
   // [ReadOnly]
   // public float waitTimeBeforeNextFx;
    [ReadOnly]
    public float waitTimeBeforeNextDamage;
    [ReadOnly]
    public List<PawnController> PawnTrapped;
    public List<GameObject> IdleFx;
    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;
    private GameObject myFx;

    // Update is called once per frame
    void Awake()
    {
        // waitTimeBeforeNextFx = 0;
        IdleFx = new List<GameObject>();
        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = puzzleData.M_PuzzleElectreticPlate;
    }

    void FixedUpdate()
    {
        
        waitTimeBeforeNextDamage -= Time.deltaTime;
        //waitTimeBeforeNextFx -= Time.deltaTime;

        if (waitTimeBeforeNextDamage < 0 && isActivated)
        {
            waitTimeBeforeNextDamage = puzzleData.timeCheckingDamageEletricPlate;
            foreach (PawnController item in PawnTrapped)
            {
                item.Damage(puzzleData.DamageEletricPlate);
            }
        }
        /*
        if (waitTimeBeforeNextFx < 0 && isActivated)
        {
            waitTimeBeforeNextFx = 0.18f ;
            if (IdleFx.Count > 4)
            {
                Destroy(IdleFx[0]);
                IdleFx.RemoveAt(0);
            }
            //Vector3 pos_Fx = new Vector3(transform.localPosition.x + Random.Range(-transform.localScale.x, transform.localScale.x) * 0.6f, transform.localPosition.y + transform.localScale.x * 0.3f, transform.localPosition.z + Random.Range(-transform.localScale.z, transform.localScale.z) * 0.6f);
            Vector3 pos_Fx = new Vector3(transform.position.x + Random.Range(-transform.lossyScale.x, transform.lossyScale.x) / 2, transform.position.y + transform.lossyScale.y * 0.3f, transform.position.z + Random.Range(-transform.lossyScale.z, transform.lossyScale.z) /2);
            IdleFx.Add(FXManager.InstantiateFX(puzzleData.ElectricPlateActivate, pos_Fx, false, Vector3.zero, Vector3.one * 0.7f));
        }
        */
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PawnController>())
        {
            
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            //pawn.Damage(puzzleData.DamageEletricPlate);
            PawnTrapped.Add(pawn);
        }

    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PawnController>())
        {
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            PawnTrapped.Remove(pawn);
        }

    }


    override public void WhenActivate()
    {
        isActivated = true;
        meshRenderer.material = puzzleData.M_PuzzleElectreticPlate_Activated;
        if (myFx != null)
        {
            Destroy(myFx);
        }
        myFx = FXManager.InstantiateFX(puzzleData.ElectricPlateActivate, transform.position, false, Vector3.zero, Vector3.one * 2.5f);
    }

    override public void WhenDesactivate()
    {
        meshRenderer.material = puzzleData.M_PuzzleElectreticPlate;
        isActivated = false;

        Destroy(myFx);

    }


}
