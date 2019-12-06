using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PuzzleActivable : MonoBehaviour
{
    public PuzzleDatas puzzleData;
    [SerializeField] public List<PuzzleActivator> puzzleActivators;
    [SerializeField] public List<PuzzleActivator> puzzleDesactivator;
    [ReadOnly] public List<bool> puzzleActivationsBool;
    [SerializeField] public bool needAllConditions = false;

    public List<Light> indictatorLightsList;
    public bool isActivated = true;


    public virtual void Start()
    {
        if (isActivated)
        {
            WhenActivate();
        }

        if (!isActivated)
        {
            WhenDesactivate();
        }
        foreach (var item in indictatorLightsList)
        {
            item.gameObject.SetActive(true);
        }


        UpdateListBool();
        if (needAllConditions)
        {
            int nbLightNeeded = 0;
            nbLightNeeded += puzzleActivationsBool.Count;
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
            //If the activable don't have needallcontions, we need only one light
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
        UpdateListBool();
        if (needAllConditions)
        {
            for (int i = 0; i < indictatorLightsList.Count; i++)
            {
                Light item = indictatorLightsList[i];
                if (item.gameObject.activeSelf)
                {
                    if (puzzleActivationsBool[i])
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
        foreach (var item in indictatorLightsList)
        {
            item.gameObject.SetActive(true);
        }


        UpdateListBool();
        if (needAllConditions)
        {
            int nbLightNeeded = 0;
            nbLightNeeded += puzzleActivationsBool.Count;
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
            //If the activable don't have needallcontions, we need only one light
            for (int i = 1; i < indictatorLightsList.Count; i++)
            {
                if (indictatorLightsList[i] != null)
                {
                    indictatorLightsList[i].gameObject.SetActive(false);
                }
            }
        }
        UpdateLights();

        return "Lights updated";
    }

    public void UpdateListBool()
    {
        puzzleActivationsBool = new List<bool>();
        foreach (var item in puzzleActivators)
        {
            if (item.isActivated)
            {
                puzzleActivationsBool.Add(true);
            }
            else
            {
                puzzleActivationsBool.Add(false);
            }
        }

        foreach (var item in puzzleDesactivator)
        {
            if (item.isActivated)
            {
                puzzleActivationsBool.Add(false);
            }
            else
            {
                puzzleActivationsBool.Add(true);
            }
        }
    }
}
