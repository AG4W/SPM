using UnityEngine;

using System.Linq;

public class WeaponDatabase : MonoBehaviour
{
    static bool hasInitialized = false;

    static Weapon[] weapons;

    public static void Initialize()
    {
        if (hasInitialized)
            return;

        weapons = Resources.LoadAll<Weapon>("Weapons/");
        hasInitialized = true;
    }

    public static Weapon Get(string type)
    {
        return (Weapon)weapons.FirstOrDefault(w => w.GetType().ToString() == type);
    }
}
