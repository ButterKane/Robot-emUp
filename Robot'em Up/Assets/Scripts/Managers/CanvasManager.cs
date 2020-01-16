using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager i; 

    public Canvas mainCanvasPrefab;
    [NonSerialized] public Canvas mainCanvas;

    private void Awake()
    {
        if (i != null) { Destroy(i); }
        i = this;

        mainCanvas = Instantiate(mainCanvasPrefab);
    }

}
