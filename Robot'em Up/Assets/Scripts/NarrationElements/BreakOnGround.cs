using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakOnGround : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Break();
        }
    }

    private void Break()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        FeedbackManager.SendFeedback("event.ScreenBreaking", this);
    }
}
