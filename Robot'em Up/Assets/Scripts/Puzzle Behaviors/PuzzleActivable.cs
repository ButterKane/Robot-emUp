using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzleActivable : MonoBehaviour
{
    public PuzzleDatas puzzleData;
    [SerializeField] public List<PuzzleActivator> puzzleActivators;
    [SerializeField] public List<PuzzleActivator> puzzleDesactivator;
    [SerializeField] public bool needAllConditions = false;

    public List<Light> indictatorLightsList;
    public bool isActivated = true;


    public virtual void Start()
    {
        if (isActivated)
        {
            WhenActivate();
        }
        foreach (var item in indictatorLightsList)
        {
            item.gameObject.SetActive(true);
        }
        
        if (needAllConditions)
        {
            int nbLightNeeded = 0;
            nbLightNeeded += puzzleActivators.Count;
            for (int i = nbLightNeeded; i < indictatorLightsList.Count; i++)
            {
                    if (indictatorLightsList[i] != null)
                    {
                        indictatorLightsList[i].gameObject.SetActive(false);
                    }
            }
        }
        else
        {
            //If the activaable don't have needallcontions, we need only one light
            for (int i = 1; i < indictatorLightsList.Count; i++)
            {
                if (indictatorLightsList[i] != null)
                {
                    indictatorLightsList[i].gameObject.SetActive(false);
                }
            }
        }
        UpdateLights();
    }


    public virtual void WhenActivate()
    {
        isActivated = true;
    }


    public virtual void WhenDesactivate()
    {
        isActivated = false;
    }


    public virtual void UpdateLights()
    {
        if (needAllConditions)
        {
            for (int i = 0; i < indictatorLightsList.Count; i++)
            {
                Light item = indictatorLightsList[i];
                if (item.gameObject.activeSelf)
                {
                    if (puzzleActivators[i].isActivated)
                    {
                        item.color = Color.green;
                    }
                    else
                    {
                        item.color = Color.red;
                    }

                }
            }
        }
        else
        {
            if (isActivated)
            {
                indictatorLightsList[0].color = Color.green;
            }
            else
            {
                indictatorLightsList[0].color = Color.red;
            }

        }


    }



    [ButtonMethod]
    public string InitializeIndicatorLight()
    {
        Start();
        return "Lights updated";
    }
}
