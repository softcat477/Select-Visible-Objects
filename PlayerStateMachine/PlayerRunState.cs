using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : IState
{
    Vector2 moveVector;
    public PlayerRunState(PlayerStateMachine psm) : base(psm) {

    }
    public override void EnterState() {
        Debug.Log("Enter Run state");
        _psm.animator.SetBool("isRunning", true);
    }
    public override void UpdateState() {
        // Vector3 dSpeed = _psm.transform.forward * _psm.runSpeed;
        // //Debug.Log(dSpeed);
        // _psm.charV.x = dSpeed.x;
        // _psm.charV.z = dSpeed.z;
        // // Slowly align to camera's front axis
        // Vector2 direction = new Vector2(_psm.transform.forward.x, _psm.transform.forward.z);
        // Vector2 cameraDirection = new Vector2(_psm.followCamera.transform.forward.x, _psm.followCamera.transform.forward.z);
        // float angleToCameraFront = Vector2.SignedAngle(direction, cameraDirection);
        // if (Mathf.Abs(angleToCameraFront) >= 1.0f) {
        //     var tmpRot = Quaternion.Euler(0, angleToCameraFront*-_psm.turnSpeed, 0);
        //     _psm.transform.rotation *= tmpRot;
        // }
        // Update animation
        // Conditions to switch state
        moveVector = _psm.ReadMoveInput();
        moveVector = moveVector.normalized;
        float mag = moveVector.magnitude;

        // transform to camera space
        Vector2 moveVectorCameraSpace = toCameraSpace(moveVector);
        Vector3 dSpeed = new Vector3(moveVectorCameraSpace.x, 0, moveVectorCameraSpace.y);
        dSpeed = dSpeed * _psm.runSpeed;
        _psm.charV.x = dSpeed.x;
        _psm.charV.z = dSpeed.z;

        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveVectorCameraSpace.x, 0, moveVectorCameraSpace.y));
        Quaternion currentRotation = _psm.transform.rotation;
        _psm.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _psm.rotatePerFrame * Time.deltaTime);
        CheckSwitchStates();
    }
    public override void ExitState() {
        _psm.animator.SetBool("isRunning", false);
    }
    public override void CheckSwitchStates() {
        if (!_psm.isRunning || !_psm.isMoving) {
            IState nextState = PlayerIdleState.Create(_psm);
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
    static public PlayerRunState Create(PlayerStateMachine psm) {
        return new PlayerRunState(psm);
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
