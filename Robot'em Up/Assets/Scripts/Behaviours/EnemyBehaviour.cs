using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public int maxHealth = 100;
    public int health;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            DamageTaken(10);
        }
    }

    public void DamageTaken(int damage)
    {
        animator.SetTrigger("Hit"); // play animation
        health -= damage;
        CheckHealth();
    }

    public void CheckHealth()
    {
        if (health <= 0)
        {
            // TODO Dead
        }
    }
}
