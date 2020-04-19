using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField]float maxHealth = 10f;
    [SerializeField]float healthRegenerationRate;
    [SerializeField]float healthRegenerationAmount;

    [SerializeField]bool isDestructible = true;

    public Vital Health { get; private set; }

    //basklass
    //kommer lite skit här sen
    void Start()
    {
        Initalize();
    }
    protected virtual void Initalize()
    {
        Health = new Vital(VitalType.Health, maxHealth, healthRegenerationRate, healthRegenerationAmount);
        Health.OnCurrentChanged += OnHealthChanged;
    }
    void Update()
    {
        Health.Tick();
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
}
