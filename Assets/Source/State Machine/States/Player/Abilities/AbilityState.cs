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

        base.Actor.Raise(ActorEvent.SetAnimatorLayer, AnimatorLayer.Force, 1f);
        base.Actor.Raise(ActorEvent.SetAnimatorFloat, "castIndex", (float)this.animationIndex);
        base.Actor.Raise(ActorEvent.SetAnimatorBool, "isCasting", true);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 0f);
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
        base.Actor.Raise(ActorEvent.SetAnimatorLayer, AnimatorLayer.Force, 0f);
        base.Actor.Raise(ActorEvent.SetAnimatorBool, "isCasting", false);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 1f);
    }
}
public enum AbilityAnimationIndex
{
    LeftHandAggressive,
    LeftHandNeutral,
}
