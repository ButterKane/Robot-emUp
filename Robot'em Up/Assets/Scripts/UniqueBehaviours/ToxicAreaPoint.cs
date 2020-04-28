using System.Collections;
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

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.GetComponent<PlayerController>())
        {
            switch (myType)
            {
                case ToxicAreaPointType.Entry:
                    ToxicAreaManager.toxicAreaManager.ToxicAreaEntry();
                    break;
                case ToxicAreaPointType.Exit:
                    ToxicAreaManager.toxicAreaManager.ToxicAreaLeaving();
                    break;
                default:
                    break;
            }

        }
    }

}
