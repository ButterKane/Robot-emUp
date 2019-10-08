using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton du gameManager
    [HideInInspector] public static GameManager i;
    [HideInInspector] public LevelManager levelManager;
    [HideInInspector] public InputManager inputManager;

    public GameObject mainCameraGO;
    public GameObject playerOne;
    public GameObject playerTwo;


    private void Awake()
    {
        i = this;
        if (levelManager == null){ levelManager = FindObjectOfType<LevelManager>();}
        if (inputManager == null) { inputManager = FindObjectOfType<InputManager>(); }

        playerOne.GetComponent<PlayerController>().otherPlayer = playerTwo.transform;
        playerTwo.GetComponent<PlayerController>().otherPlayer = playerOne.transform;
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
