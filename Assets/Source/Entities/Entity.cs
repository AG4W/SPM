using UnityEngine;

public class Entity : MonoBehaviour
{
    [TextArea(3, 10)][SerializeField]string header = "REPLACE ME";

    [SerializeField]float maxHealth = 10f;
    [SerializeField]float healthRegenerationRate;
    [SerializeField]float healthRegenerationAmount;

    [SerializeField]bool isDestructible = true;

    public Vital Health { get; private set; }

    public string Header { get { return header; } }
    //basklass
    //kommer lite skit här sen
    void Start()
    {
        Initalize();
    }
    void Update()
    {
        Health.Tick();
    }

    protected virtual void Initalize()
    {
        Health = new Vital(VitalType.Health, maxHealth, healthRegenerationRate, healthRegenerationAmount);
        Health.OnCurrentChanged += OnHealthChanged;
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
