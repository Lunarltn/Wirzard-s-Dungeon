using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    protected StateMachine stateMachine;
    protected float StateTimer;

    public virtual void Enter()
    {
        StateTimer = 0;
    }
    public virtual void Exit() { }
    public virtual void LogicUpdate() { }
    public virtual void AnimationUpdate() { }
    public virtual void PhysicsUpdate() { }
    public void LogicTimer() { StateTimer += Time.deltaTime; }
}