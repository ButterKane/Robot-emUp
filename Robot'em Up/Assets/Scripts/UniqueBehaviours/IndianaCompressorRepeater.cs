using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndianaCompressorRepeater : PuzzleRepeater
{
    public GameObject cube;
    public Transform piston;
    public LineRenderer magneticField;
    public Transform startPoint;
    public Transform endPoint;
    public float fallSpeed;
    public float reloadSpeed;
    private bool mustReload;
    public int damage;

    // Start is called before the first frame update
    public override void Start()
    {
        cube.transform.position = startPoint.position;
        timeSpeedChange = speedChange;
    }

    // Update is called once per frame
    void Update()
    {
        magneticField.SetPosition(1, new Vector3(0, cube.transform.position.y - piston.position.y, 0));

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
        cube.transform.Translate(Vector3.down * fallSpeed); // LErp from strat to end at down speed
        if (cube.transform.position.y <= endPoint.position.y )
        {
            mustReload = true;
        }
    }

    public override void DeactivatedAction()
    {
        cube.transform.Translate(Vector3.up * reloadSpeed); // LErp from end to start at slower up speed
        if (cube.transform.position.y >= startPoint.position.y)
        {
            timeSpeedChange = speedChange;
            mustReload = false;
        }
    }

    public void DetectedTouch(Collider _other)
    {
        /// TODO: Change the damage source for each one
        if (_other.gameObject.tag == "Player")
        {
            _other.gameObject.GetComponent<IHitable>().OnHit(null, cube.transform.position - _other.gameObject.transform.position, null, damage, DamageSource.RedBarrelExplosion);
        }
        if (_other.gameObject.tag == "Enemy")
        {
            _other.gameObject.GetComponent<EnemyBehaviour>().OnHit(null, cube.transform.position - _other.gameObject.transform.position, null, damage, DamageSource.RedBarrelExplosion);
        }
    }
}
