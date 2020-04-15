using UnityEngine;

using System;
using System.Collections.Generic;

[Serializable]
public class StateMachine
{
    State current;
    State next;

    //private Stack<State> automaton;
    readonly Dictionary<Type, State> states = new Dictionary<Type, State>(); // Här kommer vi åt kopiorna på alla states. Endast en typ av state skall ha en instance.

    public StateMachine(object controller, State[] states, Dictionary<string, object> context)
    {
        //onödig initialisering, de defaultar till null ovan
        //currentState = null;
        //queuedState = null;

        //glorious fori superier
        //fuck foreach
        //syntaktiskt socker för scriptkiddies
        for (int i = 0; i < states.Length; i++)
        {
            //kombinerade tre metodanrop till intialize istället
            states[i].Initialize(this, context);

            if (this.states.ContainsKey(states[i].GetType()))
                Debug.LogError("Multiple states of the same type in " + controller.ToString() + ", check Resources/States/* and remove duplicates");

            this.states.Add(states[i].GetType(), states[i]);
        }
        //foreach (State state in states)
        //{
        //State instance = UnityEngine.Object.Instantiate(state); // Vi skapar en kopia av staten i runtime
        //Undviker att kopiera, nu kan vi ändra världen i realtid istället.
        //state.SetStateMachine(this);
        //state.SetContext(context);

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
        current = states.Last();
        current.Enter();
    }

    public void TransitionTo<T>() where T : State // T måste vara ett State
    {
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
        current.Tick();

        UpdateState();
    }

    void UpdateState()
    {
        if (next != null && next != current)
        {
            current?.Exit();
            //: Pushdown automaton
            //automaton.Push(currentState);
            current = next;
            current.Enter();
        }
    }
}
