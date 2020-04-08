using UnityEngine;

public class Vital
{
    float regenerationRate;
    float regenerationAmount;

    float regenerationTimer;

    public float Current { get; private set; }
    public float Max { get; }

    public float CurrentInPercent { get { return this.Current / this.Max; } }

    public VitalType Type { get; }

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
        regenerationTimer += Time.deltaTime;

        if(regenerationTimer >= regenerationRate)
        {
            regenerationTimer = 0f;
            this.Update(regenerationAmount);
        }
    }

    public void Update(float change)
    {
        this.Current = Mathf.Clamp(this.Current + change, 0f, this.Max);
    }
}
