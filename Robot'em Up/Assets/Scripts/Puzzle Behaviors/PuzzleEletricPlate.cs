using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.Analytics;

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
        meshRenderer.material = puzzleData.m_puzzleElectreticPlate;


        PawnTrapped.Clear();
    }

    void FixedUpdate()
    {
        for (int i = 0; i < PawnTrapped.Count; i++)
        {
            PawnController item = PawnTrapped[i];
            if (item.currentHealth <1)
            {
                PawnTrapped.Remove(item);
            }
        }
        waitTimeBeforeNextDamage -= Time.deltaTime;
        //waitTimeBeforeNextFx -= Time.deltaTime;

        if (waitTimeBeforeNextDamage < 0 && !isActivated)
        {
            waitTimeBeforeNextDamage = puzzleData.timeCheckingDamageEletricPlate;
            foreach (PawnController item in PawnTrapped)
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
				item.AddSpeedCoef(new SpeedCoef(0.5f, puzzleData.timeCheckingDamageEletricPlate, SpeedMultiplierReason.Freeze, false));

				FeedbackManager.SendFeedback("event.PuzzleElectricPlateDamage", item);
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

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.GetComponent<PawnController>())
        {
            
            PawnController _pawn = _other.gameObject.GetComponent<PawnController>();
            //pawn.Damage(puzzleData.DamageEletricPlate);

            if (_pawn.ignoreEletricPlates == false)
            {
                PawnTrapped.Add(_pawn);
            }

            if (!isActivated)
            {
                _pawn.AddSpeedCoef(new SpeedCoef(0.5f, puzzleData.timeCheckingDamageEletricPlate, SpeedMultiplierReason.Freeze, false));
            }
        }

    }


    private void OnTriggerExit(Collider _other)
    {
        if (_other.gameObject.GetComponent<PawnController>())
        {
            PawnController _pawn = _other.gameObject.GetComponent<PawnController>();
            PawnTrapped.Remove(_pawn);
        }

    }



    void OnDisable()
    {
        PawnTrapped.Clear();
    }



    override public void WhenActivate()
    {
        isActivated = true;

        UpdateLights();
        meshRenderer.material = puzzleData.m_puzzleElectreticPlate;


        if (myFx != null)
        {
            Destroy(myFx);
        }

    }

    override public void WhenDesactivate()
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

            isActivated = false;
            UpdateLights();
            meshRenderer.material = puzzleData.m_puzzleElectreticPlate_Activated;

            Destroy(myFx);

            if (myFx != null)
            {
                Destroy(myFx);
            }
			myFx = FeedbackManager.SendFeedback("event.PuzzleElectricPlateActivation", this).GetVFX();
        }
    }
}
