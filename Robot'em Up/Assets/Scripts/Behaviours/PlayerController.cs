using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public Transform self;
    public Transform otherPlayer;
    public Rigidbody rb;
    public Camera cam;

    [SerializeField]
    private DummyPlayer dummyBehaviourScript;

    [Space(2)]
    [Header("General settings")]
    public bool isDummyPlayer = false;
    public int inputIndex;
    public Color playerColor;

    [Space(2)]
    [Header("Movement settings")]
    public float accelerationSpeed = 15;
    [Tooltip("Minimum required speed to go to walking state")] public float minWalkSpeed = 0.1f;
    public float maxSpeedMin = 9;
    public float maxSpeedMax = 11;
    public float maxAcceleration = 10;

    public GameObject ballPrefab;
    public Transform ballTarget;

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
        if (!isDummyPlayer)
        {
            ApplyMove();
        }
        else
        {
            dummyBehaviourScript.MoveAroundPlayer(otherPlayer);
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            var ball = Instantiate(ballPrefab);
            ball.transform.position = self.position + self.forward;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                ball.GetComponent<BallBehaviour>().ThrowCurveBall(ballTarget.position, (ballTarget.position - self.position).magnitude, self);
            }
            else
            {
                ball.GetComponent<BallBehaviour>().ThrowStraightBall(ballTarget.position, (ballTarget.position - self.position).magnitude, self);
            }
        }

        
    }
    

    void GetInput()
    {
        if (GameManager.i.inputManager.inputDisabled) { input = Vector3.zero; return; }

        input = new Vector3(Input.GetAxis("Horizontal") * accelerationSpeed * Time.deltaTime, 0, Input.GetAxis("Vertical") * accelerationSpeed * Time.deltaTime);
    }

    void GetMove()
    {
        GetInput();

        if (input.magnitude > inputManager. minJoystickStrength)
        {
            movementDirection = inputManager.GetMoveAsViewedWithCamera(input.x, input.z) ;
        }
        else
        {
            movementDirection = Vector3.zero;
            rb.velocity = Vector3.zero;
        }
    }

    void ApplyMove()
    {
        GetMove();

        // Apply the movement
        rb.AddForce(movementDirection * input.magnitude , ForceMode.VelocityChange);
    }
}
