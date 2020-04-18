using UnityEngine;

public abstract class AbilityState : ActState
{
    [SerializeField]KeyCode hotkey = KeyCode.Alpha0;
    [SerializeField]AbilityAnimationIndex animationIndex;

    [SerializeField]bool debug = true;

    protected float Timer { get; set; }

    public AbilityAnimationIndex AnimationIndex { get { return animationIndex; } }
    public bool Debug { get { return debug; } }

    public override void Enter()
    {
        base.Enter();

        this.Timer = 0f;

        base.Actor.Raise(ActorEvent.SetActorAnimatorLayer, AnimatorLayer.Force, 1f);
        base.Actor.Raise(ActorEvent.SetActorAnimatorFloat, "castIndex", (float)this.animationIndex);
        base.Actor.Raise(ActorEvent.SetActorAnimatorBool, "isCasting", true);

        base.Actor.Raise(ActorEvent.SetActorLeftHandWeight, 0f);
    }
    public override void Tick()
    {
        base.Tick();
        this.Timer += Time.deltaTime / Time.timeScale;

        if (!Input.GetKey(hotkey))
            base.Return();
    }

    public override void Exit()
    {
        base.Actor.Raise(ActorEvent.SetActorAnimatorLayer, AnimatorLayer.Force, 0f);
        base.Actor.Raise(ActorEvent.SetActorAnimatorBool, "isCasting", false);
        base.Actor.Raise(ActorEvent.SetActorLeftHandWeight, 0f);
    }
}
public enum AbilityAnimationIndex
{
    LeftHandAggressive,
    LeftHandNeutral,
}
