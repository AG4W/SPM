using System;
using System.Collections.Generic;

public static class GlobalEvents
{
    static List<Action<object[]>>[] global;
    //look upon me and despair
    static Dictionary<Actor, List<Action<object[]>>[]> actor;

    public static void Initialize()
    {
        global = new List<Action<object[]>>[Enum.GetNames(typeof(GlobalEvent)).Length];
        actor = new Dictionary<Actor, List<Action<object[]>>[]>();

        for (int i = 0; i < global.Length; i++)
            global[i] = new List<Action<object[]>>();
    }

    public static void Subscribe(GlobalEvent globalEvent, Action<object[]> action) => global[(int)globalEvent].Add(action);
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

    //oanvända, men sparar ifall de kommer behövas någon gång
    //public static void Unsubscribe(GlobalEvent e, Action<object[]> a) => events[(int)e].Remove(a);
    //public static void Unsubscribe(this Actor actor, ActorEvent e, Action<object[]> action) => actorEvents[actor][(int)e].Remove(action);
    //public static void ClearAllListeners(this Actor actor) => actorEvents.Remove(actor);
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

    //dialogue
    PlayDialogue,

    //camera && post
    SetCameraSettings,
    UpdateDOFFocusDistance,
    ModifyCameraTrauma,
    ModifyCameraTraumaCapped,

    //ui
    OnInteractableStart,
    OnInteractableComplete,
    CurrentInteractableChanged,
    SetCrosshairIcon,

    //ai
    NoiseCreated,
    AlertOthers,

    //scene management
    OnSceneLoad,

    //Skinned mesh renderer
    SetPlayerAlpha,
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

    PlayAudio,

    //animator
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
