using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class IndianaExplosion : MonoBehaviour
{
    public float myScale;
    public float waitTimeForExplosion;
    public bool isBarrage;
    [ReadOnly] public float waitingForExplosion;
    [ReadOnly]  public bool Explosed;
    
    private SpriteRenderer spriteRenderer;
    private SphereCollider sphereCollider;
    [ReadOnly] public IndianaManager indianaManager;


    private List<PawnController> listPawnsHere;

    // Start is called before the first frame update
    public void Initiate()
    {
        transform.localScale = new Vector3(myScale, myScale, myScale);
        waitingForExplosion = waitTimeForExplosion;
        listPawnsHere = new List<PawnController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        waitingForExplosion -= Time.deltaTime;


        if (!Explosed)
        {
            transform.Rotate(new Vector3(0, 0, 1), Time.deltaTime * 20f);
            spriteRenderer.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, 1 - waitingForExplosion / waitTimeForExplosion + 0.05f));
        }

        if (waitingForExplosion < 0 && !Explosed)
        {
            Explosed = true;
            GameObject FX;
            if (isBarrage)
            {
                FX = FeedbackManager.SendFeedback("event.IndianaBarrageExposion", this).GetVFX();
            }
            else
            {
                FX = FeedbackManager.SendFeedback("event.IndianaRegularExposion", this).GetVFX();
            }
            
            FX.transform.localScale = new Vector3(myScale, myScale, myScale);


            spriteRenderer.color = new Color(1, 1, 1, 0.05f);
           // fXExplosion.SetActive(true);
            foreach (var item in listPawnsHere)
            {
                item.Damage(indianaManager.damageToPawn);
            }
        }
        if (waitingForExplosion < -2)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.GetComponent<PawnController>())
        {
            PawnController pawn = _other.gameObject.GetComponent<PawnController>();
            listPawnsHere.Add(pawn);
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.GetComponent<PawnController>())
        {
            PawnController pawn = _other.gameObject.GetComponent<PawnController>();
            listPawnsHere.Remove(pawn);
        }
    }
}
