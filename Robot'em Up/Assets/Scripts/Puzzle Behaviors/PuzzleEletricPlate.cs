using System.Collections;
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
    public float speedModifier = 0.5f;
    private MeshRenderer meshRenderer;
    private GameObject myFx;

    // Update is called once per frame
    void Awake()
    {
        IdleFx = new List<GameObject>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = puzzleData.m_puzzleElectreticPlate;
        pawnTrapped.Clear();
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
            if (_pawn.ignoreEletricPlates == false)
            {
                pawnTrapped.Add(_pawn);
            }

            if (!isActivated)
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

            UpdateLights();
            meshRenderer.material = puzzleData.m_puzzleElectreticPlate;

            if (myFx != null)
            {
                Destroy(myFx);
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
                meshRenderer.material = puzzleData.m_puzzleElectreticPlate_Orange;
                StartCoroutine(GettingEletrified());
            }
        }
    }

    public IEnumerator GettingEletrified ()
    {
        yield return new WaitForSeconds(puzzleData.timeOrangePressurePlate);

        isActivated = false;
        UpdateLights();
        meshRenderer.material = puzzleData.m_puzzleElectreticPlate_Activated;

        Destroy(myFx);

        if (myFx != null)
        {
            Destroy(myFx);
        }
        myFx = FeedbackManager.SendFeedback("event.PuzzleElectricPlateActivation", this).GetVFX();
        myFx.transform.parent = transform;
    }


    public override void CustomShutDown()
    {
        meshRenderer.material = puzzleData.m_puzzleElectreticPlate_ShutDown;
        Destroy(myFx);
    }
}
