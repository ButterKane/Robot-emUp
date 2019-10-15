using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : MonoBehaviour
{
    public Transform self;
    public AnimationCurve curveBallSpeed;
    public float curveBallAvancement;
    public float straightBallAvancement;
    public Rigidbody rb;
    public GameObject thrower;
    public float magnetForceRatio = 10f;
    private int defaultLayer = 0;


    // Start is called before the first frame update
    void Start()
    {
        self = transform;
    }

    private void OnEnable()
    {
        defaultLayer = gameObject.layer;
        self = transform;
        rb = GetComponent<Rigidbody>();
        GameManager.i.ball = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnableGravity()
    {
        rb.useGravity = true;
    }

    public void DisableGravity()
    {
        rb.useGravity = false;
    }

    public void EnableCollisions()
    {
        rb.isKinematic = false;
        gameObject.layer = defaultLayer;
    }
    public void DisableCollisions()
    {
        rb.isKinematic = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

}
