using UnityEngine;

using System;
using System.Collections.Generic;

[Serializable]
public class StateMachine
{
    [SerializeField]bool debugStateMachine = false;
    Actor owner;

    readonly Stack<State> queue = new Stack<State>();
    readonly Dictionary<Type, State> states = new Dictionary<Type, State>(); // Här kommer vi åt kopiorna på alla states. Endast en typ av state skall ha en instance.

    State next;

    public State Current { get; private set; }

    public StateMachine(object controller, State[] states, Dictionary<Type, object> context, Type startState)
    {
        owner = (Actor)controller;

        //glorious fori superier
        //fuck foreach
        //syntaktiskt socker för scriptkiddies
        for (int i = 0; i < states.Length; i++)
        {
            this.states.Add(states[i].GetType(), UnityEngine.Object.Instantiate(states[i]));
            this.states[states[i].GetType()].Initialize(this, context);
        }
        //foreach (State state in states)
        //{
        //State instance = UnityEngine.Object.Instantiate(state); // Vi skapar en kopia av staten i runtime
        //Undviker att kopiera, nu kan vi ändra världen i realtid istället.
        //state.SetStateMachine(this);
        //state.SetContext(context);
        //OBS, MÅSTE ÄNDRA TILLBAKA DETTA NÄR VI LÄGGER IN AI, DÅ AIN använder flera states

        //state.Initialize();

        //stateDictionary.Add(state.GetType(), state); // ett state-typ kopplas till en faktisk state

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
