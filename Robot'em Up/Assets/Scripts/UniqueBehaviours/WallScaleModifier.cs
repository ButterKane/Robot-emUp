using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class WallScaleModifier : MonoBehaviour
{
    // Start is called before the first frame update

    public List<Vector3> listScales;
    [ReadOnly]public Vector3 wantedScale;

    void Start()
    {
        wantedScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, wantedScale, Time.deltaTime);
    }

    public void ChangeScale (int IdScale)
    {
        wantedScale = listScales[IdScale];
    }
}
