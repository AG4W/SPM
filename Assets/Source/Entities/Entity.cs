using UnityEngine;

public class Entity : MonoBehaviour, IDamageable
{
    [SerializeField]float maxHealth = 10f;
    [SerializeField]float healthRegenerationRate;
    [SerializeField]float healthRegenerationAmount;

    [SerializeField]bool isDestructible = true;

    Vital health;

    //basklass
    //kommer lite skit här sen
    void Start()
    {
        Initalize();
    }
    protected virtual void Initalize()
    {
        health = new Vital(VitalType.Health, maxHealth, healthRegenerationRate, healthRegenerationAmount);
        health.OnCurrentChanged += OnHealthChanged;
    }
    void Update()
    {
        health.Tick();
    }

    protected virtual void OnHealthChanged(float current)
    {
        if (!isDestructible)
            return;

        if (current <= 0f)
            OnHealthZero();
    }
    protected virtual void OnHealthZero()
    {
        //this.GetComponentInChildren<Animator>()?.SetTrigger("Death");
        //Destroy(this.transform.gameObject);
    }

    void IDamageable.OnHit(float damage)
    {
        health.Update(damage);
    }
}
