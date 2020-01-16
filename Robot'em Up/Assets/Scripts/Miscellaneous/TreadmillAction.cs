using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreadmillAction : MonoBehaviour
{
    public GameObject steve;
    public float treadmillForce = 10;

    private Vector3 steveDirection;
    private List<Rigidbody> _concernedRbs = new List<Rigidbody>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        steveDirection = steve.transform.forward;

        if (_concernedRbs.Count > 0)
        {
            foreach (var rb in _concernedRbs)
            {
                rb.AddForce(-rb.velocity , ForceMode.Acceleration);
                rb.AddForce(steveDirection * treadmillForce, ForceMode.Acceleration);
            }
        }
    }


    /// Right now, the detection takes the magnet collider of the player as weel, needs rework to prevent that

    private void OnTriggerEnter(Collider _other)
    {
        Rigidbody thisRb;
        bool isAlreadyPresent = false;

        if (_other.gameObject.tag == "Player" || _other.gameObject.tag == "Enemy")
        {
            if (_other.gameObject.GetComponent<Rigidbody>())
            {
                thisRb = _other.gameObject.GetComponent<Rigidbody>();
                foreach (var rb in _concernedRbs)
                {
                    if (rb == thisRb)
                    {
                        isAlreadyPresent = true;
                        break;
                    }
                }

                if (!isAlreadyPresent)
                {
                    _concernedRbs.Add(thisRb);
                }
            }
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        _concernedRbs.Remove(_other.gameObject.transform.root.GetComponent<Rigidbody>());
    }

}
