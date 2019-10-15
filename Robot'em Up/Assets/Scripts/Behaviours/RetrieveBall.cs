using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetrieveBall : MonoBehaviour
{
    public Transform parent;
    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ball")
        {
            Debug.Log("Player Trigger enter: " + other.name);

            parent.GetComponent<PlayerControllerAlex>().hasBall = true;

            BallBehaviour ballBehaviour = other.gameObject.transform.parent.GetComponent<BallBehaviour>();
            ballBehaviour.StopAllCoroutines();
            ballBehaviour.rb.velocity = Vector3.zero;
        }
    }
}
