using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArmAttack : MonoBehaviour
{
    Collider armCollider;

    // Start is called before the first frame update
    void Start()
    {
        armCollider = GetComponent<Collider>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleArmCollider(bool? value)
    {
        if(value != null)
        {
            armCollider.enabled = (bool)value;
        }
        else
        {
            armCollider.enabled = !armCollider.enabled;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            armCollider.enabled = false;
            PlayerController player = other.GetComponent<PlayerController>();
            player.Damage(10);
        }
    }
}
