using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public EnemyBehaviour parentScript;
    public GameObject head;
    
    void Update()
    {
        head.transform.LookAt(parentScript.focusedPlayer.GetCenterPosition());
    }
}
