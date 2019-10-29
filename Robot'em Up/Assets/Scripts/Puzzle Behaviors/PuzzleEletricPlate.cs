using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleEletricPlate : PuzzleActivable
{
    private float waitTimeBeforeNextFx;
    public List<GameObject> IdleFx;

    // Update is called once per frame
    void Awake()
    {
        waitTimeBeforeNextFx = 0;
        IdleFx = new List<GameObject>();
    }

    void Update()
    {
        waitTimeBeforeNextFx -= Time.deltaTime;
        if (waitTimeBeforeNextFx < 0 && isActivated)
        {
            waitTimeBeforeNextFx = 0.1f / transform.lossyScale.magnitude;
            if (IdleFx.Count > 6)
            {
                Destroy(IdleFx[0]);
                IdleFx.RemoveAt(0);
            }
            //Vector3 pos_Fx = new Vector3(transform.localPosition.x + Random.Range(-transform.localScale.x, transform.localScale.x) * 0.6f, transform.localPosition.y + transform.localScale.x * 0.3f, transform.localPosition.z + Random.Range(-transform.localScale.z, transform.localScale.z) * 0.6f);
            Vector3 pos_Fx = new Vector3(transform.position.x + Random.Range(-transform.lossyScale.x, transform.lossyScale.x) / 2, transform.position.y + transform.lossyScale.y * 0.3f, transform.position.z + Random.Range(-transform.lossyScale.z, transform.lossyScale.z) /2);
            IdleFx.Add(FXManager.InstantiateFX(puzzleData.ElectricPlateActivate, pos_Fx, false, Vector3.zero, Vector3.one * 0.7f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PawnController>() && isActivated)
        {
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            pawn.Damage(puzzleData.DamageEletricPlate);
        }

    }


    override public void WhenActivate()
    {
        isActivated = true;
    }

    override public void WhenDesactivate()
    {
        isActivated = false;
        while (IdleFx.Count > 0)
        {

            Destroy(IdleFx[0]);
            IdleFx.RemoveAt(0);
        }

    }


}
