using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public Transform self;
    public Rigidbody rb;
    public Camera cam;

    [Space(2)]
    [Header("General settings")]
    public int inputIndex;
    public Color playerColor;

    [Space(2)]
    [Header("Movement settings")]
    public float accelerationSpeed = 15;
    [Tooltip("Minimum required speed to go to walking state")] public float minWalkSpeed = 0.1f;
    public float maxSpeedMin = 9;
    public float maxSpeedMax = 11;
    public float maxAcceleration = 10;



    Vector3 input;
    InputManager inputManager;
    Vector3 movementDirection;


    // Start is called before the first frame update
    void Start()
    {
        self = GetComponent<Transform>();

        rb = GetComponent<Rigidbody>();

        cam = GameManager.i.mainCameraGO.GetComponent<Camera>();

        inputManager = GameManager.i.inputManager;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyMove();
    }
    

    void GetInput()
    {
        if (GameManager.i.inputManager.inputDisabled) { input = Vector3.zero; return; }

        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Debug.Log("input = " + input);
    }

    void GetMove()
    {
        GetInput();

        if (input.magnitude > inputManager. minJoystickStrength)
        {
            movementDirection = inputManager.GetMoveAsViewedWithCamera(input.x, input.z);
        }
    }

    void ApplyMove()
    {
        GetMove();

        // Apply the movement
        rb.AddForce(movementDirection * input.magnitude * accelerationSpeed * Time.deltaTime, ForceMode.VelocityChange);
    }
}
