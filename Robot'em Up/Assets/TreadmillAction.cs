﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreadmillAction : MonoBehaviour
{
    public GameObject Steve;
    public float TreadmillForce = 10;

    private Vector3 steveDirection;
    private List<Rigidbody> _concernedRbs = new List<Rigidbody>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        steveDirection = Steve.transform.forward;

        if (_concernedRbs.Count > 0)
        {
            foreach (var rb in _concernedRbs)
            {
                Debug.Log("Treadmill for " + rb.gameObject.name);
                rb.AddForce(-rb.velocity , ForceMode.Acceleration);
                rb.AddForce(steveDirection * TreadmillForce, ForceMode.Acceleration);
            }
        }
    }


    /// Right now, the detection takes the magnet collider of the player as weel, needs rework to prevent that

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody thisRb;
        bool isAlreadyPresent = false;
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy")
        {
            if (other.gameObject.GetComponent<Rigidbody>())
            {
                thisRb = other.gameObject.GetComponent<Rigidbody>();
                foreach (var rb in _concernedRbs)
                {
                    if (rb == thisRb)
                    {
                        Debug.Log(other.gameObject.name + " is already in the list");
                        isAlreadyPresent = true;
                        break;
                    }
                }

                if (!isAlreadyPresent)
                {
                    Debug.Log("Adding " + other.gameObject.name + " to the list");
                    _concernedRbs.Add(thisRb);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Removing " + other.gameObject.transform.root.name + " of the list");
        _concernedRbs.Remove(other.gameObject.transform.root.GetComponent<Rigidbody>());
    }

}
