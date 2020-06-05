using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputRemapLauncher : UIBehaviour
{
    public GameObject remapWindow;
    public InputRemapper inputRemapper;

    // Start is called before the first frame update
    void Start()
    {
        if (inputRemapper ==null)
        {
            inputRemapper = remapWindow.GetComponent<InputRemapper>();
        }
    }

    public override void PressingA()
    {
        remapWindow.SetActive(true);
        inputRemapper.OpenRemapInput();
    }
}
