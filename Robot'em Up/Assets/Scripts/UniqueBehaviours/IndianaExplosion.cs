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

    public GameObject FXExplosion;
    private SpriteRenderer spriteRenderer;
    private SphereCollider sphereCollider;
    [ReadOnly] public IndianaManager indianaManager;


    private List<PawnController> ListPawnsHere;

    // Start is called before the first frame update
    public void Initiate()
    {
        transform.localScale = new Vector3(myScale, myScale, myScale);
        waitingForExplosion = waitTimeForExplosion;
        ListPawnsHere = new List<PawnController>();
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
            if (!isBarrage)
            {
                indianaManager.indianaCamera.shakeAmount += 0.02f * myScale;
            }
            spriteRenderer.color = new Color(1, 1, 1, 0.05f);
            FXExplosion.SetActive(true);
            foreach (var item in ListPawnsHere)
            {
                item.Damage(indianaManager.DamageToPawn);
            }
        }
        if (waitingForExplosion < -2)
        {
            Destroy(gameObject);
        }

        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PawnController>())
        {
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            ListPawnsHere.Add(pawn);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PawnController>())
        {
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            ListPawnsHere.Remove(pawn);
        }
    }
}
