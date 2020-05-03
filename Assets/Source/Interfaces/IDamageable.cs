using UnityEngine;

public interface IDamageable
{
    IDamageableType Type { get; }

    void OnHit(float damage);
}
public enum IDamageableType
{
    Organic,
    Metallic
}
