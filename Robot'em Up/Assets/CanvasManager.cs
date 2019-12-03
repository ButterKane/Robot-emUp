using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager i; 

    [SerializeField] private Canvas mainCanvasPrefab;
    public Canvas MainCanvas;

    private void Awake()
    {
        if (i != null) { Destroy(i); }
        i = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        MainCanvas = Instantiate(mainCanvasPrefab);
    }
}
