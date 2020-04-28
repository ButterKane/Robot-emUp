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

    public Renderer weakPointIndicator;
    public float timeToFillUpIndicator;
    float indicatorFilling;
    public Transform minHeight;
    public Transform maxHeight;

    public void Awake()
    {
        weakPointIndicator.material.SetFloat("_MinHeight", minHeight.position.y);
        weakPointIndicator.material.SetFloat("_MaxHeight", maxHeight.position.y);
    }


    override public void Activate()
    {

        isActivated = true;
        UpdateLights();
        life--;
        if (life > -1)
        {
            if (life == 0)
            {
                FeedbackManager.SendFeedback("event.WeakPointDestroyed", this);
                //FXManager.InstantiateFX(explosionFx, transform.position, true, Vector3.zero, Vector3.one * 5);
                mainTurret.InverseLaser();
                //DestroyWeakPoint();
                StartCoroutine(FillIndicator());
                OldBoss_Manager.i.DestroyAWeakPoint();
                foreach (var item in puzzleActivators)
                {
                    item.ShutDownPuzzleActivator();
                }
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

    public IEnumerator FillIndicator()
    {
        indicatorFilling += Time.deltaTime/timeToFillUpIndicator;
        weakPointIndicator.material.SetFloat("_ProgressionSlider", indicatorFilling);
        yield return new WaitForEndOfFrame();
        if (indicatorFilling < 1)
        {
            StartCoroutine(FillIndicator());
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
