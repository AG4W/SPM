﻿using UnityEngine;

using System.Collections.Generic;
using System;

public abstract class State : ScriptableObject
{
    Dictionary<Type, object> context;

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
