using System.Collections.Generic;
using System;

public static class GlobalEvents
{
    static List<Action<object[]>>[] events;

    //orkade inte sätta upp en initializer, så lazy initar arrayen
    static void Initialize()
    {
        events = new List<Action<object[]>>[Enum.GetNames(typeof(GlobalEvent)).Length];

        for (int i = 0; i < events.Length; i++)
            events[i] = new List<Action<object[]>>();
    }

    public static void Subscribe(GlobalEvent e, Action<object[]> a)
    {
        if (events == null)
            Initialize();

        events[(int)e].Add(a);
    }
    public static void Raise(GlobalEvent e, params object[] args)
    {
        if (events == null)
            Initialize();

        //notera nullguard (?-operatorn) i mitten
        //kommer krascha om vi invokar en null action
        for (int i = 0; i < events[(int)e].Count; i++)
            events[(int)e][i]?.Invoke(args);
    }
}

//obs, notera att serializerade (de ni exposeat i inspectorn)/hårdkodade variabler av den här typen inte uppdateras
//ifall en ny enum läggs till, så ni behöver manuellt gå tillbaka och rätta till dem
public enum GlobalEvent
{
    OpenInteractPrompt,
    CloseInteractPrompt,
}
