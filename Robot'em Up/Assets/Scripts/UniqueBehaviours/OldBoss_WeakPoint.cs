using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldBoss_WeakPoint : PuzzleActivable
{
    // Start is called before the first frame update

    public int life = 1;
    public GameObject weakPointModel;
    public OldBoss_MainTurret mainTurret;
    public GameObject explosionFx;

    public void Awake()
    {

    }


    override public void WhenActivate()
    {

        isActivated = true;
        UpdateLights();
        life--;
        if (life > -1)
        {
            Debug.Log("Explosion");
            FXManager.InstantiateFX(explosionFx, transform.position + Vector3.up *1.5f, true, Vector3.zero, Vector3.one * 3);
            if (life == 0)
            {
                FXManager.InstantiateFX(explosionFx, transform.position, true, Vector3.zero, Vector3.one * 5);
                mainTurret.InverseLaser();
                DestroyWeakPoint();
				OldBoss_Manager.i.DestroyAWeakPoint();
            }
            else
            {
                PuzzleLink[] links = FindObjectsOfType<PuzzleLink>();
                foreach (var item in links)
                {
                    item.chargingTime = -1f;
                }
            }
            
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
