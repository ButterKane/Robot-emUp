using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzlePressurePlate : PuzzleActivator
{
    [ReadOnly] public bool pawnHere;
    private BoxCollider boxCollider;
    private List<PawnController> listPawnsHere;


    // Start is called before the first frame update
    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        listPawnsHere = new List<PawnController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.GetComponent<PawnController>())
        {
            pawnHere = true;
            transform.localScale = new Vector3(transform.localScale.x, 0.3f, transform.localScale.z);
            PawnController pawn = _other.gameObject.GetComponent<PawnController>();
            //pawn.Damage(puzzleData.DamageEletricPlate);
            listPawnsHere.Add(pawn);
            isActivated = true;
            ActivateLinkedObjects();
        }

        UpdateLight();
    }


    private void OnTriggerExit(Collider _other)
    {
        if (_other.gameObject.GetComponent<PawnController>())
        {
            PawnController pawn = _other.gameObject.GetComponent<PawnController>();
            listPawnsHere.Remove(pawn);
            if (listPawnsHere.Count < 1)
            {
                isActivated = false;
                DesactiveLinkedObjects();
                pawnHere = false;
                transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
            }
        }

        UpdateLight();

    }
    
}
