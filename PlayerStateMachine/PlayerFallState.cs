using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : IState
{
    public PlayerFallState(PlayerStateMachine psm) : base(psm) {

    }
    public override void EnterState() {
        Debug.Log("Enter Fall state");
        _psm.animator.SetBool("isFalling", true);
    }
    public override void UpdateState() {
        // Update physics
        // Update animation
        // Conditions to switch state
        CheckSwitchStates();
    }
    public override void ExitState() {
        _psm.animator.SetBool("isFalling", false);
    }
    public override void CheckSwitchStates() {
        if (_psm.isGrounded) {
            //IState nextState = PlayerLandState.Create(_psm);
            //SwitchState(nextState);
            if (_psm.isRunning) {
                IState nextState = PlayerRunState.Create(_psm);
                SwitchState(nextState);
            }
            else if (_psm.isMoving) {
                IState nextState = PlayerMoveState.Create(_psm);
                SwitchState(nextState);
            }
            else {
                IState nextState = PlayerIdleState.Create(_psm);
                SwitchState(nextState);
            }
        }
    }

    static public PlayerFallState Create(PlayerStateMachine psm) {
        return new PlayerFallState(psm);
    }
}
