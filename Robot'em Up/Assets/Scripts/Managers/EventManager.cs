using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void ButtonAction();
    public static event ButtonAction dummySpawn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(Screen.width - 100, 5, 100, 30), "Spawn Dummy"))
    //    {
    //        DummySpawn?.Invoke();
    //    }
    //}
}
