using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_MainTurret : MonoBehaviour
{
    public float speed;
    public float multiplier;
    public int DamageToPlayer;
    // Start is called before the first frame update
    void Start()
    {
        multiplier = 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + Time.deltaTime * speed * multiplier, transform.rotation.eulerAngles.z);
    }

    public void InverseLaser()
    {
        if (multiplier ==-1)
        {
            multiplier = 1;
        }
        else
        {
            multiplier = -1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PawnController>())
        {
            
            PawnController pawn = other.gameObject.GetComponent<PawnController>();
            pawn.Damage(DamageToPlayer);
        }

    }
}
