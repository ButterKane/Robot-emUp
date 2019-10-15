using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractBall : MonoBehaviour
{
    public Transform parent;
    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ball")
        {
            Debug.Log("Magnet Trigger enter: " + other.name);

            other.gameObject.transform.parent.GetComponent<BallBehaviour>().ActivateMagnet(parent);
        }
    }
}
