using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleActivator : MonoBehaviour
{
    public bool isActivated;
    public PuzzleDatas puzzleData;
    public Light indictatorLight;
    // Update is called once per frame


    public virtual void Start()
    {
        UpdateLight();
        if (isActivated)
        {
            ActivateLinkedObjects();
        }
    }


    public virtual void UpdateLight()
    {
        if (indictatorLight != null)
        {
            if (isActivated)
            {
                indictatorLight.color = Color.green;
            }
            else
            {
                indictatorLight.color = Color.red;
            }
        }
       
    }


    public virtual void ActivateLinkedObjects()
    {
        PuzzleActivable[] activables = FindObjectsOfType<PuzzleActivable>();
        //Debug.Log("Find call ");

        foreach (var item in activables)
        {
            if (item.needAllConditions == false)
            {
                if (item.puzzleActivators.Contains(this))
                {
                    item.WhenActivate();
                }
                if (item.puzzleDesactivator.Contains(this))
                {
                    item.WhenDesactivate();
                }
            }
            else
            {
                bool temp_Activated = true;
                foreach (PuzzleActivator puzzleActivator in item.puzzleActivators)
                {
                    if (!puzzleActivator.isActivated)
                    {
                        temp_Activated = false;
                    }
                }
                
                foreach (PuzzleActivator puzzleActivator in item.puzzleDesactivator)
                {
                    if (puzzleActivator.isActivated)
                    {
                        temp_Activated = false;
                    }
                }
                
                if (temp_Activated)
                {
                    item.WhenActivate();
                }


                if (!temp_Activated)
                {
                    item.WhenDesactivate();
                }


                item.UpdateLights();
            }
        }
    }



    public virtual void DesactiveLinkedObjects()
    {
        PuzzleActivable[] activables = FindObjectsOfType<PuzzleActivable>();


        foreach (var item in activables)
        {
            if (item.needAllConditions == false)
            {
                if (item.puzzleActivators.Contains(this))
                {
                    item.WhenDesactivate();
                }
                if (item.puzzleDesactivator.Contains(this))
                {
                    item.WhenActivate();
                }
            }
            else
            {
                bool temp_Activated = true;
                foreach (var puzzleActivator in item.puzzleActivators)
                {
                    if (isActivated)
                    {
                        temp_Activated = false;
                    }
                }

                foreach (var puzzleActivator in item.puzzleDesactivator)
                {
                    if (!isActivated)
                    {
                        temp_Activated = false;
                    }
                }
                if (temp_Activated)
                {
                    item.WhenDesactivate();
                }


            }

            item.UpdateLights();
        }

    }
}
