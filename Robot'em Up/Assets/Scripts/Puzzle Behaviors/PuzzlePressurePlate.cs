using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePressurePlate : PuzzleActivator
{
    private bool PlayerHere;
    private BoxCollider boxCollider;
    // Start is called before the first frame update
    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!PlayerHere)
        {
            if (other.GetComponent<PlayerController>())
            {
                PlayerHere = true;
                ActivateLinkedObjects();
                transform.localScale = new Vector3(transform.localScale.x, 0.1f, transform.localScale.z);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (PlayerHere)
        {
            if (other.GetComponent<PlayerController>())
            {
                DesactiveLinkedObjects();
                PlayerHere = false;
                transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
            }
        }
    }
}
