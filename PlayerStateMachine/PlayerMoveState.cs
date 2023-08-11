using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : IState
{
    Vector2 moveVector;
    public PlayerMoveState(PlayerStateMachine psm) : base(psm) {

    }
    public override void EnterState() {
        Debug.Log("Enter Move state");
        moveVector = _psm.ReadMoveInput();
        float mag = moveVector.magnitude;

        //float speed = Remap(mag, 0.3f, 1.0f, 1.0f, 1.5f);
        _psm.animator.SetFloat("Velocity", mag);
        _psm.animator.SetBool("isWalking", true);
    }
    public override void UpdateState() {
        // Update physics
        moveVector = _psm.ReadMoveInput();
        float mag = moveVector.magnitude;

        mag = Mathf.Clamp(mag, 0.3f, 1.0f);
        if (mag < 0.3f) mag = 0.0f;
        _psm.animator.SetFloat("Velocity", mag);

        // transform to camera space
        Vector2 moveVectorCameraSpace = toCameraSpace(moveVector);
        Vector3 dSpeed = new Vector3(moveVectorCameraSpace.x, 0, moveVectorCameraSpace.y);
        dSpeed = dSpeed * _psm.forwardSpeed;
        _psm.charV.x = dSpeed.x * mag;
        _psm.charV.z = dSpeed.z * mag;

        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveVectorCameraSpace.x, 0, moveVectorCameraSpace.y));
        Quaternion currentRotation = _psm.transform.rotation;
        _psm.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _psm.rotatePerFrame * Time.deltaTime);

        CheckSwitchStates();
    }
    public override void ExitState() {
        Debug.Log("Exit Move State");
        _psm.animator.SetFloat("Velocity", 0);
        _psm.animator.SetBool("isWalking", false);
    }
    public override void CheckSwitchStates() {
        if (!_psm.isMoving) {
            // pass
            IState nextState = PlayerIdleState.Create(_psm); 
            SwitchState(nextState);
        }
        else if (_psm.isRunning) {
            IState nextState = PlayerRunState.Create(_psm); 
            SwitchState(nextState);
        }
        else if (_psm.isJumping) {
            IState nextState = PlayerJumpState.Create(_psm);
            SwitchState(nextState);
        }
        else if (!_psm.isGrounded) {
            IState nextState = PlayerFallState.Create(_psm);
            SwitchState(nextState);
        }
    }

    public float Remap(float value, float minCurrent, float maxCurrent, float minDesired, float maxDesired)
    {
        return minDesired + (value - minCurrent) * (maxDesired - minDesired) / (maxCurrent - minCurrent);
    }

    static public PlayerMoveState Create(PlayerStateMachine psm) {
        return new PlayerMoveState(psm);
    }

    private Vector2 toCameraSpace(Vector2 vec) {
        Vector3 flat_world_forward = new Vector3(Vector3.forward.x, 0, Vector3.forward.z);
        Vector3 flat_camera_forward = new Vector3(_psm.followCamera.transform.forward.x, 0, _psm.followCamera.transform.forward.z);

        Quaternion rot = Quaternion.FromToRotation(flat_world_forward, flat_camera_forward);
        Vector3 before = new Vector3(vec.x, 0, vec.y);
        Vector3 after = rot * before;
        return new Vector2(after.x, after.z);
    }

}
