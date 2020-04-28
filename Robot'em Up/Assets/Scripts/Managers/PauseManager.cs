using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private float initialTimeScale;
    private bool isgamePaused;

    // Start is called before the first frame update
    void Start()
    {
        isgamePaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseGame();
        }
    }

    void TogglePauseGame()
    {
        isgamePaused = !isgamePaused;
        switch(isgamePaused)
        {
            case true:
                ResumeGame();
                break;
            case false:
                PauseGame();
                break;
        }
    }

    public void PauseGame()
    {
        initialTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        if (initialTimeScale != 0)
            Time.timeScale = initialTimeScale;
        else
            Time.timeScale = GameManager.i.gameSpeed;
    }

}
