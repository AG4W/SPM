/* 
 * This class is the base for the callback system
 */

using System;
using System.Collections.Generic;

public static class GlobalEvents
{
    /// <summary>
    /// An array of Lists containing the Subscribed GlobalEvents
    /// </summary>
    static List<Action<object[]>>[] global;

    /// <summary>
    /// A Dictionary with every Subscribed Actor keyed to an array of Lists containing the Subscribed GlobalEvents
    /// </summary>
    static Dictionary<Actor, List<Action<object[]>>[]> actor;

    /// <summary>
    /// Initializes the GlobalEvents system. Must be done before listeners are Subscribed.
    /// </summary>
    public static void Initialize()
    {
        global = new List<Action<object[]>>[Enum.GetNames(typeof(GlobalEvent)).Length];
        actor = new Dictionary<Actor, List<Action<object[]>>[]>();

        for (int i = 0; i < global.Length; i++)
            global[i] = new List<Action<object[]>>();
    }

    /// <summary>
    /// Subscribe a listener for a GlobalEvent.
    /// </summary>
    /// <param name="globalEvent">The Event name(Enum).</param>
    /// <param name="action">The action/actions to perform when the event is raised.</param>
    public static void Subscribe(GlobalEvent globalEvent, Action<object[]> action) => global[(int)globalEvent].Add(action);

    /// <summary>
    /// Subscribe a listener for a ActorEvent.
    /// </summary>
    /// <param name="actor">The Actor in question.</param>
    /// <param name="actorEvent">The Event name(Enum).</param>
    /// <param name="action">The action/actions to perform when the event is raised.</param>
    public static void Subscribe(this Actor actor, ActorEvent actorEvent, Action<object[]> action)
    {
        //skapa ny slot ifall actor inte finns
        if (!GlobalEvents.actor.ContainsKey(actor))
        {
            List<Action<object[]>>[] entries = new List<Action<object[]>>[Enum.GetNames(typeof(ActorEvent)).Length];

            for (int i = 0; i < entries.Length; i++)
                entries[i] = new List<Action<object[]>>();

            GlobalEvents.actor.Add(actor, entries);
        }

        GlobalEvents.actor[actor][(int)actorEvent].Add(action);
    }

    /// <summary>
    /// Raise/call the GlobalEvent with the desired arguments.
    /// </summary>
    /// <param name="globalEvent">The Event name(Enum).</param>
    /// <param name="args">The desired arguments.</param>
    public static void Raise(GlobalEvent globalEvent, params object[] args)
    {
        for (int i = 0; i < global[(int)globalEvent].Count; i++)
        {
            //kommer nullreffa om vi invokar en null action
            //också dumt om vi har tomma subscribes, så ta bort dem
            if (global[(int)globalEvent][i] == null)
                global[(int)globalEvent].RemoveAt(i);

            global[(int)globalEvent][i].Invoke(args);
        }
    }

    //vem behöver koll på vad som skickas vart oavsett
    public static void Raise(this Actor actor, ActorEvent actorEvent, params object[] args)
    {
        for (int i = 0; i < GlobalEvents.actor[actor][(int)actorEvent].Count; i++)
        {
            if (GlobalEvents.actor[actor][(int)actorEvent][i] == null)
                GlobalEvents.actor[actor][(int)actorEvent].RemoveAt(i);

            GlobalEvents.actor[actor][(int)actorEvent][i].Invoke(args);
        }
    }

    // Oanvända, men sparar ifall de kommer behövas någon gång
    //public static void Unsubscribe(GlobalEvent e, Action<object[]> a) => events[(int)e].Remove(a);
    //public static void Unsubscribe(this Actor actor, ActorEvent e, Action<object[]> action) => actorEvents[actor][(int)e].Remove(action);
    //public static void ClearAllListeners(this Actor actor) => actorEvents.Remove(actor);
}

//obs, notera att serializerade (de ni exposeat i inspectorn)/hårdkodade variabler av den här typen inte uppdateras
//ifall en ny enum läggs till, så ni behöver manuellt gå tillbaka och rätta till dem
public enum GlobalEvent
{
    // Player
    SetPlayerWeapon,
    PlayerHealthChanged,
    PlayerForceChanged,
    PlayerShotHit,
    PlayerShotMissed,
    ToggleTorches,
    OnAbilityActivated,
    UpdateForce,

    // Generic
    OnIDamageableHit,

    // Audio stuff
    PlayShotSFX,
    PlayImpactSFX,

    // Dialogue
    PlayDialogue,

    // Camera && post
    SetCameraMode,
    UpdateDOFFocusDistance,
    ModifyCameraTrauma,
    ModifyCameraTraumaCapped,

    // UI
    OnInteractableStart,
    OnInteractableComplete,
    CurrentInteractableChanged,
    SetCrosshairIcon,

    // AI
    NoiseCreated,
    AlertOthers,

    // Scene management
    OnSceneLoad,
}
public enum ActorEvent
{
    // Actor input
    SetTargetInput,
    SetInputModifier,
    SetTargetStance,
    SetTargetAimMode,
    SetTargetWeaponIndex,
    SetTargetPosition,
    SetTargetRotation,

    // Audio stuff
    PlayAudio,

    // Animator actor
    SetAnimatorFloat,
    SetAnimatorTrigger,
    SetAnimatorBool,
    SetAnimatorLayer,

    UpdateGroundedStatus,

    // Vitals
    OnActorHealthChanged,

    // Weapon
    FireWeapon,
    ReloadWeapon,
    SetWeapon,
    ShotHit,
    ShotMissed,

    // Velocity
    ModifyVelocity,

    // IK
    SetLookAtPosition,
    SetLookAtWeights,
    SetLeftHandTarget,
    SetLeftHandWeight,

    // AI
    UpdateAITargetStatus,
    SetLastKnownPositionOfTarget,
    OnAIForceAffectStart,
    OnAIForceAffectEnd,
}
