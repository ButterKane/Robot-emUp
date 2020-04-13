using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExoticExplosionTrigger : MonoBehaviour
{
    bool explosionInitiated;
    public float myScale;
    public float waitTimeForExplosion;
    float waitingForExplosion;
    bool Explosed;

    private List<PawnController> listPawnsHere;

    public Transform innerCircleTransform;
    public float targetScale;

    // Start is called before the first frame update
    public void Initiate()
    {
        transform.localScale = Vector3.one * myScale;
        waitingForExplosion = waitTimeForExplosion;
        listPawnsHere = new List<PawnController>();
        explosionInitiated = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (explosionInitiated)
        {
            waitingForExplosion -= Time.deltaTime;


            if (!Explosed)
            {
                innerCircleTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * targetScale, (waitTimeForExplosion - waitingForExplosion) / waitTimeForExplosion);
            }

            if (waitingForExplosion < 0 && !Explosed)
            {
                Explosed = true;
                GameObject FX = FeedbackManager.SendFeedback("event.IndianaRegularExposion", this).GetVFX();
                FX.transform.localScale = new Vector3(myScale, myScale, myScale);
                foreach (var item in listPawnsHere)
                {
                    item.Damage(15);
                    item.Push(PushType.Light, item.transform.position - transform.position, PushForce.Force2);
                }
            }
            if (waitingForExplosion < -0.45f)
            {
                Destroy(gameObject);
            }
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
