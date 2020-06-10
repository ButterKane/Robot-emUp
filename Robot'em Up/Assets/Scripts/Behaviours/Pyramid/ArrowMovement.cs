using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
    public float Speed;
    public float Amplitude;
    public float cooldown;
    private Vector3 StartingLocation;
    public Vector3 MovementDirection;
    // Start is called before the first frame update
    void Start()
    {
        cooldown = 0;
        StartingLocation = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + (Speed * MovementDirection);
        cooldown -= Time.deltaTime;
        if (cooldown <= 0)
        {
            cooldown = Amplitude;
            transform.position = StartingLocation;
        }
    }
}
