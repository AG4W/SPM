using UnityEngine;

public class HitboxCallbackController : MonoBehaviour, IDamageable
{
    [SerializeField]string optionalHeader = "replace me";
    [Range(0f, 5f)][SerializeField]float damageModifier = 1f;

    IDamageableType IDamageable.Type => IDamageableType.Organic;

    void Awake()
    {
        Debug.Assert(this.GetComponent<Collider>() != null, this.transform.name + " is missing a collider, a collider is required to detect hits!", this.gameObject);
    }

    void IDamageable.OnHit(float damage)
    {
        OnHit?.Invoke(damage * damageModifier);
    }

    public delegate void OnHitEvent(float damage);
    public OnHitEvent OnHit;
}