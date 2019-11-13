using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoHelper : MonoBehaviour
{

    public PuzzleDatas puzzleData;
    public GameObject myText;
    private List<PawnController> ListPawnsHere;
    // Start is called before the first frame update
    void Start()
    {
        myText.gameObject.SetActive(false);
        ListPawnsHere = new List<PawnController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            ListPawnsHere.Add(pawn);
            if (puzzleData.showTuto)
            {
                myText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            ListPawnsHere.Remove(pawn);
            if (ListPawnsHere.Count < 1)
            {
                myText.SetActive(false);
            }
        }
    }
}
