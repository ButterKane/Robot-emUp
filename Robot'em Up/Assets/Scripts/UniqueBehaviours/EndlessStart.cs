using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void  ActiveEndlessUI()
    {
        EndlessUI.instance.ShowUI();
    }
}
