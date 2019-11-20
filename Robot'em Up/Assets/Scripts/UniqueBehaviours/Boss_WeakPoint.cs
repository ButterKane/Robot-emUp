using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_WeakPoint : PuzzleActivable
{
    // Start is called before the first frame update


    public GameObject weakPointModel;
    public GameObject explosionFx;

    override public void WhenActivate()
    {
        isActivated = true;
        UpdateLights();
        DestroyWeakPoint();

    }

    public void DestroyWeakPoint()
    {
        if (weakPointModel != null)
        {
            Destroy(weakPointModel);
            FXManager.InstantiateFX(explosionFx, Vector3.up * 1, true, Vector3.zero, Vector3.one * 2);
        }
    }
}
