using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class IndianaExplosion : MonoBehaviour
{
    public float myScale;
    public float waitTimeForExplosion;
    [ReadOnly] public float waitingForExplosion;
    [ReadOnly]  public bool Explosed;

    public GameObject FXExplosion;
    private SpriteRenderer spriteRenderer;
    private SphereCollider sphereCollider;


    private List<PawnController> ListPawnsHere;

    // Start is called before the first frame update
    void Start()
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
        }

        if (waitingForExplosion < 0 && !Explosed)
        {
            Explosed = true;
            spriteRenderer.color = new Color(1, 1, 1, 0.05f);
            FXExplosion.SetActive(true);
            foreach (var item in ListPawnsHere)
            {
                item.Damage(item.maxHealth / 2);
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
