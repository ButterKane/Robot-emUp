using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzlePressurePlate : PuzzleActivator
{
    [ReadOnly]
    public bool PawnHere;
    [ReadOnly]
    public bool noPlayerHere;
    private BoxCollider boxCollider;
    public List<PawnController> ListPawnsHere;


    // Start is called before the first frame update
    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PawnController>())
        {
            PawnHere = true;
            transform.localScale = new Vector3(transform.localScale.x, 0.3f, transform.localScale.z);
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            //pawn.Damage(puzzleData.DamageEletricPlate);
            ListPawnsHere.Add(pawn);
            isActivated = true;
            ActivateLinkedObjects();
        }

    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PawnController>())
        {
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            ListPawnsHere.Remove(pawn);
            if (ListPawnsHere.Count < 1)
            {
                DesactiveLinkedObjects();
                PawnHere = false;
                transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
                isActivated = false;
            }
        }

    }
    
}
