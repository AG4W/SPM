using UnityEngine;

using System;
using System.Collections.Generic;

[Serializable]
public class StateMachine
{
    [SerializeField]bool debugStateMachine = false;
    HumanoidActor owner; 

    State next;

    //private Stack<State> automaton;
    Dictionary<Type, State> states = new Dictionary<Type, State>(); // Här kommer vi åt kopiorna på alla states. Endast en typ av state skall ha en instance.

    public State Current { get; private set; }

    public StateMachine(object controller, State[] states, Dictionary<string, object> context, Type startState)
    {
        owner = (HumanoidActor)controller;

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

        // Detta borde alltid ske, eller?
        // Vi kan väl inte manuellt assignea ett state i början,
        // Så känns weird att guarda för det
        // also, detta sker för varje state? Så de skriver över varandra??
        // Oklart
        //if (currentState == null)
        //{
        //    Debug.Log("Current state blir instance.");
        //    currentState = state;
        //}
        //}


        // Varför behövs denna nullguard?
        // Känns som att det är bättre om vi kastar exceptions i loggen
        // ifall något viktigt saknar states så att vi märker felet innan build
        //currentState?.Enter(); // Om jag fick ett state, starta den

        this.Current = this.states[startState];
        this.Current.Enter();
    }

    public void TransitionTo<T>() where T : State // T måste vara ett State
    {
        if (this.debugStateMachine)
            Debug.Log(owner.name + ": " + Current.GetType() + " -> " + next.GetType());

        next = states[typeof(T)];
    }
    public void TransitionBack()
    {
        // NOTE(Fors): Pushdown automaton
        /*
        if (automaton.Count != 0)
            queuedState = automaton.Pop();
            */
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
            //: Pushdown automaton
            //automaton.Push(currentState);
            this.Current = next;
            this.Current.Enter();
        }
    }
}
