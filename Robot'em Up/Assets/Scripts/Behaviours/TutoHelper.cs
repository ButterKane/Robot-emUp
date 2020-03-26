using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoHelper : MonoBehaviour
{

    public PuzzleDatas puzzleData;
    public GameObject myText;
    public GameObject mySprite;
    public bool GivePlayerEnergy = false;
    private List<PawnController> listPawnsHere;
    // Start is called before the first frame update
    void Start()
    {
        myText.gameObject.SetActive(false);
        mySprite.gameObject.SetActive(false);
        listPawnsHere = new List<PawnController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GivePlayerEnergy && listPawnsHere.Count > 0)
        {
            EnergyManager.IncreaseEnergy(Time.deltaTime);
        }
    }


    private void OnTriggerEnter(Collider _other)
    {
        if (_other.GetComponent<PlayerController>())
        {
            PawnController i_pawn = _other.gameObject.GetComponent<PawnController>();
            listPawnsHere.Add(i_pawn);
            if (puzzleData.showTuto)
            {
                myText.SetActive(true);
                mySprite.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.GetComponent<PlayerController>())
        {
            PawnController i_pawn = _other.gameObject.GetComponent<PawnController>();
            listPawnsHere.Remove(i_pawn);
            if (listPawnsHere.Count < 1)
            {
                myText.SetActive(false);
                mySprite.SetActive(false);
            }
        }
    }
}
