using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

public static class PersistentData
{
    //använd en double dictionary så att vi kan göra en lookup per scen, per IPersistable-objekt.
    //en datafil per scen, basically, sen klumpar vi dem till en komplett sparfil tillsammans med annan ev. data som typ high scores etc
    static Dictionary<int, Dictionary<string, Context>> data;

    public static void Initialize()
    {
        GlobalEvents.Subscribe(GlobalEvent.OnSceneEnter, OnSceneEnter);
        GlobalEvents.Subscribe(GlobalEvent.OnSceneExit, OnSceneExit);

        //hitta något sätt att regga till Application.Quit()
        //så att vi kan spara data ifrån minne till disk

        //initialize anropas varje scen (detta kanske ej behövs i final build, men whatevs
        //för komplett sparsystem, ingen data laddad i minne && ingen data på disk = ny sparfil
        //i framtiden, kolla ifall det finns data på disk vid någon angiven directory /shrug, och isåfall ladda den
        data = data ?? new Dictionary<int, Dictionary<string, Context>>();
    }

    static void OnSceneEnter(object[] args)
    {
        int sceneIndex = ((Scene)args[0]).buildIndex;

        if (!data.ContainsKey(sceneIndex))
            data.Add(sceneIndex, new Dictionary<string, Context>());

        //hitta alla persistables, och sätt dem till rätt state
        MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            IPersistable persistable = behaviours[i].GetComponent<IPersistable>();

            if (persistable != null && persistable.IsPersistable && data[sceneIndex].ContainsKey(persistable.Hash))
                persistable.OnEnter(data[sceneIndex][persistable.Hash]);
        }
    }
    static void OnSceneExit(object[] args)
    {
        int sceneIndex = ((Scene)args[0]).buildIndex;
        data[sceneIndex].Clear();

        MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            IPersistable persistable = behaviours[i].GetComponent<IPersistable>();

            if (persistable != null && persistable.IsPersistable)
                data[sceneIndex].Add(persistable.Hash, persistable.GetContext());
        }
    }
}
