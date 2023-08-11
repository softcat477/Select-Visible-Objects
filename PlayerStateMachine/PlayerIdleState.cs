using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : IState
{
    //public PlayerIdleState(PlayerStateMachine psm, StateFactory factory) : base(psm, factory) {
    public PlayerIdleState(PlayerStateMachine psm) : base(psm) {

    }
    public override void EnterState() {
        Debug.Log("Enter Idle state");
        _psm.charV.x = 0.0f;
        _psm.charV.z = 0.0f;
        _psm.animator.SetFloat("Velocity", 0.0f);
    }
    public override void UpdateState() {
        // Update physics
        // Update animation
        // Conditions to switch state
        CheckSwitchStates();
    }
    public override void ExitState() {
        Debug.Log("Exit Idle state");
    }
    public override void CheckSwitchStates() {
        if (_psm.isMoving) {
            IState nextState = StateFactory.Move(_psm);
            SwitchState(nextState);
        }
        else if (_psm.isJumping) {
            IState nextState = StateFactory.Jump(_psm);
            SwitchState(nextState);
        }
        else if (_psm.isStrafing) {
            IState nextState = PlayerStrafeState.Create(_psm);
            SwitchState(nextState);
        }
    }

    static public PlayerIdleState Create(PlayerStateMachine psm) {
        return new PlayerIdleState(psm);
    }
}
