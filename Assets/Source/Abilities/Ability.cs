using UnityEngine;

[System.Serializable]
public abstract class Ability 
{
    [SerializeField]float cooldown;
    [SerializeField]float duration;

    [SerializeField]AbilityAnimationIndex animationIndex;

    protected float Duration { get { return duration; } }
    protected float DurationTimer01 { get { return Mathf.InverseLerp(0f, this.Duration, DurationTimer); } }

    protected float CooldownTimer { get; private set; }
    protected float DurationTimer { get; private set; }

    public AbilityAnimationIndex AnimationIndex { get { return animationIndex;} }

    public bool HasCooldown { get; private set; }
    public bool IsActive { get; private set; }

    public virtual void Activate()
    {
        GlobalEvents.Raise(GlobalEvent.OnAbilityActivated, this);
        IsActive = true;
    }

    public virtual void Tick()
    {
        DurationTimer += Time.deltaTime;

        //if we've reached max duration
        if (DurationTimer >= duration)
        {
            DurationTimer = 0f;
            HasCooldown = true;
            Deactivate();
        }

        if (HasCooldown)
        {
            CooldownTimer += Time.deltaTime;

            if (CooldownTimer >= cooldown)
            {
                HasCooldown = false;
                CooldownTimer = 0f;
            }
        }
    }

    public virtual void Deactivate()
    {
        DurationTimer = 0f;
        IsActive = false;
    }
}
public enum AbilityAnimationIndex
{
    LeftHandAggressive,
    LeftHandNeutral,
}