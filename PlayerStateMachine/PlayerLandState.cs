using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandState : IState
{
    public PlayerLandState(PlayerStateMachine psm) : base(psm) {
    }
    public override void EnterState() {
        Debug.Log("Enter Land state");
        _psm.animator.SetBool("isLanding", true);
        _psm.animator.SetBool("isLandingAnimationFinished", false);
    }

    // IEnumerator DampToZero() {
    //     // pass
    // }
    public override void UpdateState() {
        // Update physics
        // Update animation
        // Conditions to switch state
        CheckSwitchStates();
    }
    public override void ExitState() {
        _psm.animator.SetBool("isLanding", false);
        _psm.animator.SetBool("isLandingAnimationFinished", true);
    }
    public override void CheckSwitchStates() {
        AnimatorStateInfo info = _psm.animator.GetCurrentAnimatorStateInfo(0);
        float playRatio = info.normalizedTime;
        if (info.IsName("Landing")) {
            Debug.Log("Landing with ratio " + playRatio);
        }
        else if (info.IsName("Falling")) {
            Debug.Log("Falling with ratio " + playRatio);
            return;
        }
        bool tmp = false;
        if (playRatio >= 1.0f) { // if meet transition offset
            // if (_psm.isMoving) {
            if (tmp) {
                IState nextState = StateFactory.Move(_psm);
                _psm.animator.SetFloat("Velocity", 0.12f);
                SwitchState(nextState);
            }
            else {
                if (info.IsName("Falling")) Debug.Log("Transition from Falling to Idle with ratio " + playRatio);
                else if (info.IsName("Landing")) Debug.Log("Transition from Landing to Idle with ratio " + playRatio);
                else Debug.Log("Transition from ??? to Idle with ratio " + playRatio);
                IState nextState = StateFactory.Idle(_psm);
                _psm.animator.SetFloat("Velocity", 0.0f);
                SwitchState(nextState);
            }
        }
    }

    static public PlayerLandState Create(PlayerStateMachine psm) {
        return new PlayerLandState(psm);
    }
}
