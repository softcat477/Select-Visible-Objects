using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IState
{
    protected PlayerStateMachine _psm;
    // protected StateFactory _factory;
    //public IState(PlayerStateMachine psm, StateFactory factory) {
    public IState(PlayerStateMachine psm) {
        _psm = psm;
    }
    public abstract void EnterState();
    public abstract void UpdateState(); // callsed by monobehavior's update function
    public abstract void ExitState();
    public abstract void CheckSwitchStates();

    protected void SwitchState(IState newState) {
        newState.EnterState();
        ExitState();
        // TODO: assign new state to the current state variable
        _psm.currentState = newState;
    }
}
