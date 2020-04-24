using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField]float maxHealth = 10f;
    [SerializeField]float healthRegenerationRate;
    [SerializeField]float healthRegenerationAmount;

    [SerializeField]bool isDestructible = true;

    Vital health;

    HitboxCallbackController[] hitboxes;

    public float HealthInPercent { get { return health.CurrentInPercent; } }

    //basklass
    //kommer lite skit här sen
    void Start()
    {
        Initalize();
    }
    protected virtual void Initalize()
    {
        hitboxes = this.GetComponentsInChildren<HitboxCallbackController>();

        for (int i = 0; i < hitboxes.Length; i++)
            hitboxes[i].OnHit += OnHit;

        health = new Vital(VitalType.Health, maxHealth, healthRegenerationRate, healthRegenerationAmount);
        health.OnCurrentChanged += OnHealthChanged;
    }
    protected virtual void Update()
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

    void OnHit(float damage)
    {
        health.Update(damage);
    }
}
