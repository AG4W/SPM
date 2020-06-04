/// <summary>
/// This class handles the persistable items in the scene
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

public static class PersistentData
{
    //använd en double dictionary så att vi kan göra en lookup per scen, per IPersistable-objekt.
    //en datafil per scen, basically, sen klumpar vi dem till en komplett sparfil tillsammans med annan ev. data som typ high scores etc
    static Dictionary<int, Dictionary<string, Context>> data;
    //static Dictionary<string, Context> scenePersistableData;

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
        //scenePersistableData = scenePersistableData ?? new Dictionary<string, Context>();
    }

    static void OnSceneEnter(object[] args)
    {
        List<IPersistable> alreadyVisited = new List<IPersistable>();
        // The input sceneIndex
        int sceneIndex = ((Scene)args[0]).buildIndex;

        // Add this scene to data if it doesn't exist 
        if (!data.ContainsKey(sceneIndex))
            data.Add(sceneIndex, new Dictionary<string, Context>());

        // Add the scenePersistables to data
        //foreach (var item in scenePersistableData)
        //{
        //    if (data[sceneIndex].ContainsKey(item.Key))
        //        data[sceneIndex][item.Key] = item.Value;
        //    else
        //        data[sceneIndex].Add(item.Key, item.Value);
        //}

        // Find all the persistables and set them to their correct state
        MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            IPersistable persistable = behaviours[i].GetComponent<IPersistable>();

            if (persistable != null && persistable.IsPersistable && data[sceneIndex].ContainsKey(persistable.Hash) && !alreadyVisited.Contains(persistable))
            {
                persistable.OnEnter(data[sceneIndex][persistable.Hash]);
                alreadyVisited.Add(persistable);
            }
        }
    }
    static void OnSceneExit(object[] args)
    {
        List<IPersistable> alreadyVisited = new List<IPersistable>();
        //scenePersistableData.Clear();
        int sceneIndex = ((Scene)args[0]).buildIndex;

        data[sceneIndex].Clear();

        MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>();

        // Saves all the persistable and scenePersistable stuff in containers
        for (int i = 0; i < behaviours.Length; i++)
        {
            IPersistable persistable = behaviours[i].GetComponent<IPersistable>();

            if (persistable != null && persistable.IsPersistable && !data[sceneIndex].ContainsKey(persistable.Hash) && !alreadyVisited.Contains(persistable))
            {
                data[sceneIndex].Add(persistable.Hash, persistable.GetContext());
                alreadyVisited.Add(persistable);

                //if (persistable.PersistBetweenScenes)
                //    scenePersistableData.Add(persistable.Hash, persistable.GetContext());
            }
        }
    }
}
