using UnityEngine;

using System;
using System.Collections.Generic;

[Serializable]
public class StateMachine
{
    [SerializeField]bool debugStateMachine = false;
    [SerializeField]bool enableRealtimeEditing = false;

    Actor owner;

    readonly Stack<State> queue = new Stack<State>();
    readonly Dictionary<Type, State> states = new Dictionary<Type, State>(); // Här kommer vi åt kopiorna på alla states. Endast en typ av state skall ha en instance.

    State next;

    public State Current { get; private set; }

    public void Initialize(Actor controller, State[] states, Dictionary<Type, object> context, Type startState)
    {
        owner = controller;

        //glorious fori superier
        //fuck foreach
        //syntaktiskt socker för scriptkiddies
        for (int i = 0; i < states.Length; i++)
        {
            this.states.Add(states[i].GetType(), enableRealtimeEditing ? states[i] : UnityEngine.Object.Instantiate(states[i]));
            this.states[states[i].GetType()].Initialize(this, context);
        }

        this.Current = this.states[startState];
        this.Current.Enter();
    }

    public void TransitionTo<T>() where T : State // T måste vara ett State
    {
        next = states[typeof(T)];

        if (this.debugStateMachine)
            Debug.Log(owner.name + ": " + Current.GetType() + " -> " + next.GetType());
    }
    public void Return()
    {
        if (queue.Count > 0)
            next = queue.Pop();

        if (this.debugStateMachine)
            Debug.Log(owner.name + ": " + Current.GetType() + " <-> " + next.GetType());
    }

    public void Tick()
    {
        this.Current.Tick();

        UpdateState();
    }

    void UpdateState()
    {
        if (next != null && next != Current)
        {
            this.Current?.Exit();

            if(this.Current != null)
                queue.Push(this.Current);
            
            this.Current = next;
            this.Current.Enter();
        }
    }
}
