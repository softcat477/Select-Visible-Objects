using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    public IState currentState {get; set;}

    PlayerInputActions playerInputActions;
    InputAction move;
    InputAction run;
    InputAction jump;
    InputAction strafe;

    public float gravity = -9.8f;

    [SerializeField] public bool isRunning = false;
    public bool isMoving {get; set;} = false;
    public bool isJumping {get; set;} = false;
    public bool isGrounded = false;
    public bool isStrafing = false;

    public float turnSpeed = 0.1f;
    public float rotatePerFrame = 5.0f;
    public float jumpSpeed = 10.0f;
    public float forwardSpeed = 10.0f;
    public float runSpeed = 15.0f;
    public float strafeSpeed = 15.0f;

    public Animator animator;
    public CharacterController character;
    public GameObject followCamera;

    public Vector3 velocity = new Vector3(0,0,0);
    public Vector3 charV;

    public GameObject select;
    [SerializeField] SelectObjectOnScreen selectObjectOnScreen;

    public GameObject cameraFreeLook;
    public GameObject cameraFocus;

    public Vector2 ReadMoveInput() {
        return move.ReadValue<Vector2>();
    }

    private void Awake() {
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable() {
        move = playerInputActions.Player.Move;
        run = playerInputActions.Player.Run;
        jump = playerInputActions.Player.Jump;
        strafe = playerInputActions.Player.Strafe;

        move = playerInputActions.Player.Move;
        move.Enable();
        move.performed += (InputAction.CallbackContext ctx) => {
            isMoving = true;
        };
        move.canceled += (InputAction.CallbackContext ctx) => {
            isMoving = false;
        };

        run = playerInputActions.Player.Run;
        run.Enable();
        run.performed += (InputAction.CallbackContext ctx) => {
            isRunning = true;
        };
        run.canceled += (InputAction.CallbackContext ctx) => {
            isRunning = false;
        };

        jump = playerInputActions.Player.Jump;
        jump.performed += (InputAction.CallbackContext ctx) => {
            isJumping = true;
        };
        jump.canceled += (InputAction.CallbackContext ctx) => {
            isJumping = false;
        };
        jump.Enable();

        strafe = playerInputActions.Player.Strafe;
        strafe.performed += (InputAction.CallbackContext ctx) => {
            isStrafing = true;
        };
        strafe.canceled += (InputAction.CallbackContext ctx) => {
            isStrafing = false;
        };
        strafe.Enable();

        selectObjectOnScreen.UpdateSelectedEvent += (GameObject sel) => {
            select = sel;
        };
        // public delegate void UpdateSelectedEventDelegate(GameObject sel);
        // public event UpdateSelectedEventDelegate UpdateSelectedEvent;
    }

    private void OnDisable() {
        move.Disable();
        run.Disable();
        jump.Disable();
    }

    private void Start() {
        currentState = StateFactory.Idle(this);
        currentState.EnterState();

        Quaternion rot = Quaternion.FromToRotation(
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1)
        );
        Vector3 before = new Vector3(-1, 0, 1);
        Vector3 after = rot * before;
        string s = $"Rotate from {before} to {after}";
        Debug.Log(s);
    }

    private void Update() {
        charV = character.velocity;

        currentState.UpdateState();

        charV.y = charV.y + gravity * Time.deltaTime;
        float dy = charV.y * Time.deltaTime;
        float dx = charV.x * Time.deltaTime;
        float dz = charV.z * Time.deltaTime;
        Vector3 movement = new Vector3(dx, dy, dz);
        //Debug.Log("Movement: " + movement + " v: " + charV);
        var flag = character.Move(new Vector3(dx, dy, dz));

        if (charV.y >= 0.1f || charV.y <= -0.1f) {
            isGrounded = false;
        }
        if ((flag & CollisionFlags.Below) != 0) {
            charV.y = 0.0f;
            isGrounded = true;
        }
    }
}
