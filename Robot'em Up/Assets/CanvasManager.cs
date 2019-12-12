using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager i; 

    public Canvas mainCanvasPrefab;
    public Canvas MainCanvas;

    private void Awake()
    {
        if (i != null) { Destroy(i); }
        i = this;

        MainCanvas = Instantiate(mainCanvasPrefab);
    }

}
