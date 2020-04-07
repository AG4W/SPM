using UnityEngine;

using System;

public class Actor : Entity
{
    [SerializeField]float maxHealth;
    [SerializeField]float healthRegenerationRate;
    [SerializeField]float healthRegenerationAmount;

    Vital[] vitals;

    public Transform FocusPoint { get; private set; }

    // Använd inte Start/Awake i klasser som ärver ifrån Entity!
    // Använd istället protected override base.Initialize(), denna kallas i Start.
    // Kom ihåg att anropa basmetoden också.
    protected override void Initalize()
    {
        base.Initalize();

        vitals = new Vital[Enum.GetNames(typeof(VitalType)).Length];
        vitals[(int)VitalType.Health] = new Vital(VitalType.Health, maxHealth, healthRegenerationRate, healthRegenerationAmount);

        this.FocusPoint = this.transform.Find("focusPoint");
        if (this.FocusPoint == null)
        {
            Debug.LogWarning(this.name + " is missing a focusPoint, using position of transform");
            this.FocusPoint = new GameObject("focusPoint").transform;
            this.FocusPoint.SetParent(this.transform);
            this.FocusPoint.position = this.transform.position;
        }
    }

    void Update()
    {
        UpdateVitals();
    }

    void UpdateVitals()
    {
        for (int i = 0; i < vitals.Length; i++)
            vitals[i].Tick();
    }

    public Vital GetVital(VitalType type)
    {
        return vitals[(int)type];
    }
}
public enum VitalType
{
    Health,
}
