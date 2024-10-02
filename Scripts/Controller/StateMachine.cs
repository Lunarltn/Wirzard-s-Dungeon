using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public State CurrentState { get; private set; }

    public void Init(State defaultState)
    {
        CurrentState = defaultState;
        CurrentState.Enter();
    }

    public void ChangeState(State state)
    {
        if (CurrentState == state)
            return;
        CurrentState.Exit();
        CurrentState = state;
        CurrentState.Enter();
    }
}
