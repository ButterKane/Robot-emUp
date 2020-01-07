using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager i;

    private void Awake()
    {
        if (i == null)
        {
            i = this;
        }
        else
        {
            Debug.LogError("There is already a LevelManager");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
