using UnityEngine;

[System.Serializable]
public abstract class Ability 
{
    [SerializeField]float cooldown;

    float cooldownTimer;

    public bool hasCooldown { get; private set; }

    public virtual void Activate(Context context)
    {
        hasCooldown = true;
    }

    public virtual void Tick()
    {
        if (!hasCooldown)
            return;

        cooldownTimer += Time.deltaTime;

        if(cooldownTimer >= cooldown)
        {
            hasCooldown = false;
            cooldownTimer = 0f;
        }
    }
}
