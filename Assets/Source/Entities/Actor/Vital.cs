using UnityEngine;

public class Vital
{
    float regenerationRate;
    float regenerationAmount;

    float regenerationTimer;

    public float Current { get; private set; }
    public float Max { get; }


    public float LatestChange { get; private set; } = 0f;
    public float CurrentInPercent { get { return this.Current / this.Max; } }
    public VitalType Type { get; }
    public bool HasReachedZero { get; private set; } = false;

    public Vital(VitalType type, float maxValue, float regenerationRate, float regenerationAmount)
    {
        this.regenerationRate = regenerationRate;
        this.regenerationAmount = regenerationAmount;

        this.Type = type;
        this.Max = maxValue;
        this.Current = this.Max;
    }

    public void Tick()
    {
        if (regenerationAmount <= 0f)
            return;

        regenerationTimer += Time.deltaTime;

        if(regenerationTimer >= regenerationRate)
        {
            regenerationTimer = 0f;
            this.Update(regenerationAmount * (1f + (1f - this.CurrentInPercent)));
        }
    }

    public void Update(float change)
    {
        this.Current = Mathf.Clamp(this.Current + change, 0f, this.Max);
        this.LatestChange = change;

        //har race conditions här ifall något gör damage flera gånger samma frame
        //se till att vi bara triggar zero events en gång
        if(!HasReachedZero)
            OnCurrentChanged?.Invoke(change);

        if (this.Current <= 0f)
            HasReachedZero = true;
    }
    public void Reset()
    {
        this.Current = this.Max;
        this.HasReachedZero = false;
        this.LatestChange = this.Max;

        OnCurrentChanged?.Invoke(this.Current);
    }

    public delegate void OnValueChanged(float change);
    public OnValueChanged OnCurrentChanged;
}
public enum VitalType
{
    Health,
}