using UnityEngine;

[System.Serializable]
public abstract class Ability 
{
    [SerializeField]float cooldown;
    [SerializeField]float duration;

    protected float Duration { get { return duration; } }
    protected float DurationTimer01 { get { return Mathf.InverseLerp(0f, this.Duration, DurationTimer); } }

    protected float CooldownTimer { get; private set; }
    protected float DurationTimer { get; private set; }

    public bool HasCooldown { get; private set; }
    public bool IsActive { get; private set; }

    public virtual void Activate(Context context)
    {
        HasCooldown = true;
        IsActive = true;
    }

    public virtual void Tick()
    {
        if (IsActive)
        {
            DurationTimer += Time.deltaTime;

            if (DurationTimer >= duration)
            {
                IsActive = false;
                DurationTimer = 0f;
            }
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
}
