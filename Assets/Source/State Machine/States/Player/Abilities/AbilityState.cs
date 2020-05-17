using UnityEngine;

public abstract class AbilityState : ActState
{
    [SerializeField]KeyCode hotkey = KeyCode.Alpha0;
    [SerializeField]AbilityAnimationIndex animationIndex;

    [Tooltip("Gets divided by deltaTime")][SerializeField]float castingCost = 1f;
    [SerializeField]float maxTrauma = .4f;

    [SerializeField]AudioClip[] activateSounds;

    protected float Timer { get; set; }

    public AbilityAnimationIndex AnimationIndex { get { return animationIndex; } }
    public AudioClip[] ActivateSounds => activateSounds;

    public override void Enter()
    {
        base.Enter();

        this.Timer = 0f;

        base.Actor.Raise(ActorEvent.SetAnimatorLayer, AnimatorLayer.Force, 1f);
        base.Animator.SetFloat("castIndex", (float)this.animationIndex);
        base.Animator.SetBool("isCasting", true);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 0f);

        GlobalEvents.Raise(GlobalEvent.OnAbilityActivated, this);
    }
    public override void Tick()
    {
        base.Tick();
        this.Timer += Time.deltaTime / Time.timeScale;

        base.Player.Force.Update(-castingCost * (Time.deltaTime / Time.timeScale));

        if (!Input.GetKey(hotkey) || base.Player.Force.CurrentInPercent <= .01f)
            base.Return();
    }

    public override void Exit()
    {
        base.Actor.Raise(ActorEvent.SetAnimatorLayer, AnimatorLayer.Force, 0f);
        base.Animator.SetBool("isCasting", false);
        base.Actor.Raise(ActorEvent.SetLeftHandWeight, 1f);
    }
}
public enum AbilityAnimationIndex
{
    LeftHandAggressive,
    LeftHandNeutral,
}
