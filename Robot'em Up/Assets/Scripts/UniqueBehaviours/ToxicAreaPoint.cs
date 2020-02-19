﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicAreaPoint : MonoBehaviour
{
    public ToxicAreaManager manager;
    public ToxicAreaPointType myType;
    public enum ToxicAreaPointType
    {
        Entry,
        Exit
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.GetComponent<PlayerController>())
        {
            switch (myType)
            {
                case ToxicAreaPointType.Entry:
                    manager.ToxicAreaEntry();
                    break;
                case ToxicAreaPointType.Exit:
                    manager.ToxicAreaLeaving();
                    break;
                default:
                    break;
            }

        }
    }

}
