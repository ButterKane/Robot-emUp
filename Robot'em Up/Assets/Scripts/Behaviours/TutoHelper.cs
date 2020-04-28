using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoHelper : MonoBehaviour
{

    public PuzzleDatas puzzleData;
    public GameObject myText;
    public GameObject mySprite;
    public bool GivePlayerEnergy = false;
    private int pawnCount;
    // Start is called before the first frame update
    void Start()
    {
        myText.gameObject.SetActive(false);
        mySprite.gameObject.SetActive(false);
        pawnCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (GivePlayerEnergy && pawnCount > 0)
        {
            EnergyManager.IncreaseEnergy(Time.deltaTime);
        }
    }


    private void OnTriggerEnter(Collider _other)
    {
        if (_other.GetComponent<PlayerController>())
        {
            pawnCount++;
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
            pawnCount--;
            if (pawnCount < 1)
            {
                myText.SetActive(false);
                mySprite.SetActive(false);
            }
        }
    }
}
