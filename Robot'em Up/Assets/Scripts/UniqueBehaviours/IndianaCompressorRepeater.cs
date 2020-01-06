using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndianaCompressorRepeater : PuzzleRepeater
{
    public GameObject Cube;
    public Transform Piston;
    public LineRenderer MagneticField;
    public Transform StartPoint;
    public Transform EndPoint;
    public float FallSpeed;
    public float ReloadSpeed;
    private bool mustReload;
    public int Damage;

    // Start is called before the first frame update
    public override void Start()
    {
        Cube.transform.position = StartPoint.position;
        timeSpeedChange = speedChange;
    }

    // Update is called once per frame
    void Update()
    {
        MagneticField.SetPosition(1, new Vector3(0, Cube.transform.position.y - Piston.position.y, 0));

        timeSpeedChange -= Time.deltaTime;

        if (timeSpeedChange < 0)
        {
            if (!mustReload)
                ActivatedAction();
            else
                DeactivatedAction();
        }
    }

    public override void ActivatedAction()
    {
        Cube.transform.Translate(Vector3.down * FallSpeed); // LErp from strat to end at down speed
        if (Cube.transform.position.y <= EndPoint.position.y )
        {
            mustReload = true;
        }
    }

    public override void DeactivatedAction()
    {
        Cube.transform.Translate(Vector3.up * ReloadSpeed); // LErp from end to start at slower up speed
        if (Cube.transform.position.y >= StartPoint.position.y)
        {
            timeSpeedChange = speedChange;
            mustReload = false;
        }
    }

    public void DetectedTouch(Collider c)
    {
        /// TODO: Change the damage source for each one
        if (c.gameObject.tag == "Player")
        {
            c.gameObject.GetComponent<IHitable>().OnHit(null, Cube.transform.position - c.gameObject.transform.position, null, Damage, DamageSource.RedBarrelExplosion);
        }
        if (c.gameObject.tag == "Enemy")
        {
            c.gameObject.GetComponent<EnemyBehaviour>().OnHit(null, Cube.transform.position - c.gameObject.transform.position, null, Damage, DamageSource.RedBarrelExplosion);
        }
    }
}
