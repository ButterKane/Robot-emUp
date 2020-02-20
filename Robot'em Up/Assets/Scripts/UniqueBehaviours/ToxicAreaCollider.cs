using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicAreaCollider : MonoBehaviour
{
    public ToxicAreaManager manager;
    public float multiplicator = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay (Collider _other)
    {

        if (_other.gameObject.GetComponent<PlayerController>())
        {
            if (_other.gameObject.GetComponent<PlayerController>().playerIndex == XInputDotNetPure.PlayerIndex.One)
            {
                manager.ToxicValue_P1 += Time.deltaTime * multiplicator;
            }
            if (_other.gameObject.GetComponent<PlayerController>().playerIndex == XInputDotNetPure.PlayerIndex.Two)
            {
                manager.ToxicValue_P2 += Time.deltaTime * multiplicator;
            }
        }
  }
}
