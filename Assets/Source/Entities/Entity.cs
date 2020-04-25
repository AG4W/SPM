using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField]float maxHealth = 10f;
    [SerializeField]float healthRegenerationRate;
    [SerializeField]float healthRegenerationAmount;

    [SerializeField]bool isDestructible = true;
    HitboxCallbackController[] hitboxes;

    public Vital Health { get; private set; }

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

        Health = new Vital(VitalType.Health, maxHealth, healthRegenerationRate, healthRegenerationAmount);
        Health.OnCurrentChanged += OnHealthChanged;
    }
    protected virtual void Update()
    {
        Health.Tick();
    }

    protected virtual void OnHealthChanged(float change)
    {
        if (!isDestructible)
            return;

        if (Health.Current <= 0f)
            OnHealthZero();
    }
    protected virtual void OnHealthZero()
    {
        //this.GetComponentInChildren<Animator>()?.SetTrigger("Death");
        //Destroy(this.transform.gameObject);
    }

    void OnHit(float damage)
    {
        Health.Update(damage);
    }
}
