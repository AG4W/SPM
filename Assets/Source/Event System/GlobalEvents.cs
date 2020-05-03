using System;
using System.Collections.Generic;

public static class GlobalEvents
{
    static List<Action<object[]>>[] events;
    static Dictionary<Actor, List<Action<object[]>>[]> actorEvents;

    //orkade inte sätta upp en initializer, så lazy initar arrayen
    static void Initialize()
    {
        events = new List<Action<object[]>>[Enum.GetNames(typeof(GlobalEvent)).Length];
        actorEvents = new Dictionary<Actor, List<Action<object[]>>[]>();

        for (int i = 0; i < events.Length; i++)
            events[i] = new List<Action<object[]>>();
    }

    public static void Subscribe(GlobalEvent e, Action<object[]> a)
    {
        if (events == null)
            Initialize();

        events[(int)e].Add(a);
    }
    public static void Unsubscribe(GlobalEvent e, Action<object[]> a)
    {
        events[(int)e].Remove(a);
    }


    //look upon me and despair
    public static void Subscribe(this Actor actor, ActorEvent e, Action<object[]> action)
    {
        if (actorEvents == null)
            Initialize();

        //skapa ny slot ifall actor inte finns
        if (!actorEvents.ContainsKey(actor))
        {
            List<Action<object[]>>[] entries = new List<Action<object[]>>[Enum.GetNames(typeof(ActorEvent)).Length];

            for (int i = 0; i < entries.Length; i++)
                entries[i] = new List<Action<object[]>>();

            actorEvents.Add(actor, entries);
        }

        actorEvents[actor][(int)e].Add(action);
    }
    public static void Unsubscribe(this Actor actor, ActorEvent e, Action<object[]> action)
    {
        actorEvents[actor][(int)e].Remove(action);
    }
    public static void ClearAllListeners(this Actor actor)
    {
        actorEvents.Remove(actor);
    }

    public static void Raise(GlobalEvent e, params object[] args)
    {
        if (events == null)
            Initialize();

        for (int i = 0; i < events[(int)e].Count; i++)
        {
            //kommer krascha om vi invokar en null action
            //också dumt om vi har tomma subscribes, så ta bort dem
            if (events[(int)e][i] == null)
                events[(int)e].RemoveAt(i);

            events[(int)e][i].Invoke(args);
        }
    }
    //vem behöver koll på vad som skickas vart oavsett
    public static void Raise(this Actor actor, ActorEvent e, params object[] args)
    {
        if (actorEvents == null)
            Initialize();

        for (int i = 0; i < actorEvents[actor][(int)e].Count; i++)
        {
            if (actorEvents[actor][(int)e][i] == null)
                actorEvents[actor][(int)e].RemoveAt(i);

            actorEvents[actor][(int)e][i].Invoke(args);
        }
    }
}

//obs, notera att serializerade (de ni exposeat i inspectorn)/hårdkodade variabler av den här typen inte uppdateras
//ifall en ny enum läggs till, så ni behöver manuellt gå tillbaka och rätta till dem
public enum GlobalEvent
{
    //player
    SetPlayerWeapon,
    PlayerHealthChanged,
    PlayerForceChanged,
    PlayerShotHit,
    PlayerShotMissed,
    ToggleTorches,
    OnAbilityActivated,
    UpdateForce,

    //generic
    OnIDamageableHit,

    //audio stuff
    PlayShotSFX,
    PlayImpactSFX,

    //camera && post
    SetCameraMode,
    UpdateDOFFocusDistance,
    ModifyCameraTrauma,
    ModifyCameraTraumaCapped,

    //ui
    OnInteractableStart,
    OnInteractableComplete,
    CurrentInteractableChanged,

    //ai
    NoiseCreated,
    AlertOthers,
}
public enum ActorEvent
{
    //actor input
    SetTargetInput,
    SetInputModifier,
    SetTargetStance,
    SetTargetAimMode,
    SetTargetWeaponIndex,
    SetTargetPosition,
    SetTargetRotation,
    UpdateAlertStatus,

    PlayAudio,

    //animator actor
    SetAnimatorFloat,
    SetAnimatorTrigger,
    SetAnimatorBool,
    SetAnimatorLayer,

    UpdateGroundedStatus,

    //vitals
    OnActorHealthChanged,

    //weapon
    FireWeapon,
    ReloadWeapon,
    SetWeapon,
    ShotHit,
    ShotMissed,

    //velocity
    ModifyVelocity,

    //IK
    SetLookAtPosition,
    SetLookAtWeights,
    SetLeftHandTarget,
    SetLeftHandWeight,

    //AI
    UpdateAITargetStatus,
    SetLastKnownPositionOfTarget,
    OnAIForceAffectStart,
    OnAIForceAffectEnd,
}
