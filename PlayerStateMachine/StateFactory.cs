using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateFactory
{
    //public PlayerStateMachine psm {get; set;}

    static public IState Idle(PlayerStateMachine psm) {
        return new PlayerIdleState(psm);
    }

    static public IState Move(PlayerStateMachine psm) {
        return new PlayerMoveState(psm);
    }

    static public IState Jump(PlayerStateMachine psm) {
        return new PlayerJumpState(psm);
    }
}