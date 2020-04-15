using UnityEngine;

using System.Collections.Generic;

public abstract class State : ScriptableObject
{
    public Dictionary<string, object> Context { get; private set; }
    
    //lazy properties
    public LocomotionController Controller { get { return (LocomotionController)this.Context["controller"]; } }
    public Transform Transform { get { return this.Controller.transform; } }

    public StateMachine StateMachine { get; private set; }

    public abstract void Initialize();

    public virtual void Enter()
    {
        Debug.Log("Entering " + this.GetType());
    }
    public abstract void Tick();
    public abstract void Exit();

    public void SetStateMachine(StateMachine sm) => this.StateMachine = sm;
    public void SetContext(Dictionary<string, object> context) => this.Context = context;

    public void TransitionTo<T>() where T : State
    {
        this.StateMachine.TransitionTo<T>();
    }
}
