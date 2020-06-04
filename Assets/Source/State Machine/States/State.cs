using UnityEngine;

using System.Collections.Generic;
using System;

public abstract class State : ScriptableObject
{
    Dictionary<Type, object> context;

    protected Camera Camera { get; private set; }

    //lazy properties
    public Actor Actor { get; private set; }
    public Transform Transform { get { return this.Actor.transform; } }

    public StateMachine StateMachine { get; private set; }
    public bool IsActiveState { get { return this.StateMachine.Current == this; } }


    public void Initialize(StateMachine stateMachine, Dictionary<Type, object> context)
    {
        this.StateMachine = stateMachine;

        this.context = context;
        this.Actor = this.Get<Actor>();

        //stoppa något retarded ifrån att inte kalla init-funktionen i subklasser
        //mha stäng init och sen overrideable OnInit-funktion istället.
        OnInitialize();
    }
    protected virtual void OnInitialize()
    {
        this.Camera = Camera.main;
    }

    public virtual void Enter()
    {
    }
    public abstract void Tick();
    public abstract void Exit();

    public void TransitionTo<T>() where T : State
    {
        this.StateMachine.TransitionTo<T>();
    }
    public void Return()
    {
        this.StateMachine.Return();
    }

    //hämtar ur context via type istället för string, så vi kan få autocasting istället för att manuellt behöva göra det.
    protected T Get<T>()
    {
        return (T)this.context[typeof(T)];
    }
}
