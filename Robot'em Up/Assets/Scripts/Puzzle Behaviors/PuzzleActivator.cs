using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleActivator : MonoBehaviour
{
    public bool isActivated;
    public PuzzleDatas puzzleData;
    public Light indictatorLight;
	// Update is called once per frame
	[HideInInspector]public Wire wire;


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

	public virtual void ActivateLinkedObjects ()
	{
		if (wire != null)
		{
			wire.ActivateWire(ActivateLinkedObjectsCallback);
		} else
		{
			ActivateLinkedObjectsCallback();
		}
	}

	public void ActivateLinkedObjectsCallback() {
		PuzzleActivable[] i_activables = FindObjectsOfType<PuzzleActivable>();
        //Debug.Log("Find call ");

        foreach (var item in i_activables)
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
                item.UpdateListBool();
                if (!item.puzzleActivationsBool.Contains(false))
                {
                    item.WhenActivate();
                }

                if (!item.puzzleActivationsBool.Contains(true))
                {
                    item.WhenDesactivate();
                }

                item.UpdateLights();
            }
        }
    }



	public virtual void DesactiveLinkedObjects ()
	{
		if (wire != null)
		{
			wire.DesactivateWire(DesactiveLinkedObjectsCallback);
		} else
		{
			DesactiveLinkedObjectsCallback();
		}
	}

	public void DesactiveLinkedObjectsCallback()
	{
		PuzzleActivable[] i_activables = FindObjectsOfType<PuzzleActivable>();

        foreach (var item in i_activables)
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
                item.UpdateListBool();
                if (!item.puzzleActivationsBool.Contains(false))
                {
                    item.WhenActivate();
                }

                if (!item.puzzleActivationsBool.Contains(true))
                {
                    item.WhenDesactivate();
                }
            }

            item.UpdateLights();
        }

    }
}
