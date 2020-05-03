using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

using System;

public static class DamageableController
{
    static GameObject[][] prefabs;

    public static void Initialize()
    {
        prefabs = new GameObject[Enum.GetNames(typeof(IDamageableType)).Length][];

        for (int i = 0; i < prefabs.Length; i++)
            prefabs[i] = Resources.LoadAll<GameObject>("VFX/" + (IDamageableType)i);

        GlobalEvents.Subscribe(GlobalEvent.OnIDamageableHit, OnHit);
    }

    static void OnHit(object[] args) => Object.Instantiate(prefabs[(int)(args[0] as IDamageable).Type].Random(), (Vector3)args[1], Random.rotation, null);
}
