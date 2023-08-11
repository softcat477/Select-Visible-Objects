using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : IState
{
    public PlayerJumpState(PlayerStateMachine psm) : base(psm) {

    }
    public override void EnterState() {
        Debug.Log("Enter JUMP state");
        _psm.charV.y = _psm.jumpSpeed;
        _psm.animator.SetBool("isJumping", true);
        _psm.isGrounded = false;
    }
    public override void UpdateState() {
        // Update physics
        // Update animation
        // Conditions to switch state
        CheckSwitchStates();
    }
    public override void ExitState() {
        _psm.animator.SetBool("isJumping", false);
    }
    public override void CheckSwitchStates() {
        if (_psm.charV.y <= -0.05f) {
            IState nextState = PlayerFallState.Create(_psm);
            SwitchState(nextState);
        }
    }
    static public PlayerJumpState Create(PlayerStateMachine psm) {
        return new PlayerJumpState(psm);
    }
}
