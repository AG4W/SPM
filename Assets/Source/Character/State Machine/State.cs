using UnityEngine;

using System.Collections.Generic;

public abstract class State : ScriptableObject
{
    public Dictionary<string, object> Context { get; private set; }
    
    //lazy properties
    public LocomotionController Controller { get { return (LocomotionController)this.Context["controller"]; } }
    public Transform Transform { get { return this.Controller.transform; } }

    public StateMachine StateMachine { get; private set; }
    public bool IsActiveState { get { return this.StateMachine.Current == this; } }

    public void Initialize(StateMachine stateMachine, Dictionary<string, object> context)
    {
        this.StateMachine = stateMachine;
        this.Context = context;

        //stoppa något retarded ifrån att inte kalla init-funktionen i subklasser
        //mha stäng init och sen overrideable OnInit-funktion istället.
        OnInitialize();
    }
    protected virtual void OnInitialize()
    {

    }

    public virtual void Enter()
    {
        //Debug.Log("Entering " + this.GetType());
    }
    public abstract void Tick();
    public abstract void Exit();

    public void TransitionTo<T>() where T : State
    {
        this.StateMachine.TransitionTo<T>();
    }
}
