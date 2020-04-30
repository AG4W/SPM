using System;

public static class Player
{
    static Weapon[] slots;

    //behöver lazy load när vi inte har en default starting screen där vi kan inita
    static bool hasInitialized = false;

    static void Initialize(SaveData save)
    {
        slots = new Weapon[Enum.GetNames(typeof(WeaponSlot)).Length];
        hasInitialized = true;
    }

    public static void SetWeapon(WeaponSlot slot, Weapon weapon)
    {
        if (!hasInitialized)
            Initialize(null);

        slots[(int)slot] = weapon;
    }
    public static Weapon GetWeapon(WeaponSlot slot)
    {
        if (!hasInitialized)
            Initialize(null);

        return slots[(int)slot];
    }
}
public enum WeaponSlot
{
    Primary,
    Secondary,
    //Tertiary
    //Gadget?
}
