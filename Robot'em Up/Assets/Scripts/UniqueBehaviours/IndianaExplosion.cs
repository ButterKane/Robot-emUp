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
    private SphereCollider sphereCollider;
    [ReadOnly] public IndianaManager indianaManager;


    private List<PawnController> listPawnsHere;

    public Transform innerCircleTransform;
    public float targetScale;

    // Start is called before the first frame update
    public void Initiate()
    {
        transform.localScale = Vector3.one * myScale;
        waitingForExplosion = waitTimeForExplosion;
        listPawnsHere = new List<PawnController>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        waitingForExplosion -= Time.deltaTime;


        if (!Explosed)
        {
            innerCircleTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * targetScale, (waitTimeForExplosion - waitingForExplosion) / waitTimeForExplosion);
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
           // fXExplosion.SetActive(true);
            foreach (var item in listPawnsHere)
            {
                item.Damage(indianaManager.damageToPawn);
                item.Push(PushType.Light, item.transform.position - transform.position, PushForce.Force2);
            }
        }
        if (waitingForExplosion < -0.45f)
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
