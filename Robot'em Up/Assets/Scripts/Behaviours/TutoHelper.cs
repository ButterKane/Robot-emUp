using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoHelper : MonoBehaviour
{

    public PuzzleDatas puzzleData;
    public GameObject myText;
    private List<PawnController> listPawnsHere;
    // Start is called before the first frame update
    void Start()
    {
        myText.gameObject.SetActive(false);
        listPawnsHere = new List<PawnController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider _other)
    {
        if (_other.GetComponent<PlayerController>())
        {
            PawnController internal_pawn = _other.gameObject.GetComponent<PawnController>();
            listPawnsHere.Add(internal_pawn);
            if (puzzleData.showTuto)
            {
                myText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.GetComponent<PlayerController>())
        {
            PawnController internal_pawn = _other.gameObject.GetComponent<PawnController>();
            listPawnsHere.Remove(internal_pawn);
            if (listPawnsHere.Count < 1)
            {
                myText.SetActive(false);
            }
        }
    }
}
