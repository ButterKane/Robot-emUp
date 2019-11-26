using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_WeakPoint : PuzzleActivable
{
    // Start is called before the first frame update


    public int life = 1;
    public GameObject weakPointModel;
    public GameObject explosionFx;

    override public void WhenActivate()
    {
        isActivated = true;
        UpdateLights();

        FXManager.InstantiateFX(explosionFx, Vector3.up * 1, true, Vector3.zero, Vector3.one * 2);
        life--;
        if (life < 1)
        {
            DestroyWeakPoint();
            FXManager.InstantiateFX(explosionFx, Vector3.up * 1, true, Vector3.zero, Vector3.one * 3);
        }

        PuzzleLink[] links = FindObjectsOfType<PuzzleLink>();
        foreach (var item in links)
        {
            item.chargingTime = -1f;
        }
    }

    public void DestroyWeakPoint()
    {
        if (weakPointModel != null)
        {
            Destroy(weakPointModel);
        }
    }
}
