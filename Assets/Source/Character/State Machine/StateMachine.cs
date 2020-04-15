using UnityEngine;

using System;
using System.Collections.Generic;

[Serializable]
public class StateMachine
{
    [SerializeField]State[] states;

    State currentState;
    State queuedState;

    //private Stack<State> automaton;
    Dictionary<Type, State> stateDictionary = new Dictionary<Type, State>(); // Här kommer vi åt kopiorna på alla states. Endast en typ av state skall ha en instance.

    public void Initialize(object controller, Dictionary<string, object> context)
    {
        currentState = null;
        queuedState = null;

        foreach (State state in states)
        {
            State instance = UnityEngine.Object.Instantiate(state); // Vi skapar en kopia av staten i runtime

            instance.SetStateMachine(this);
            instance.SetContext(context);

            instance.Initialize();

            stateDictionary.Add(instance.GetType(), instance); // Här läggs kopian till i Dictionary, ett state-typ kopplas till en faktisk state

            if (currentState == null)
            {
                Debug.Log("Current state blir instance.");
                currentState = instance;
            }
        }

        currentState?.Enter(); // Om jag fick ett state, starta den
    }

    public void TransitionTo<T>() where T : State // T måste vara ett State
    {
        queuedState = stateDictionary[typeof(T)];
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
        currentState.Tick();

        UpdateState();
    }

    void UpdateState()
    {
        if (queuedState != null && queuedState != currentState)
        {
            currentState?.Exit();
            //: Pushdown automaton
            //automaton.Push(currentState);
            currentState = queuedState;
            currentState.Enter();
        }
    }
}
