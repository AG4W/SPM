using UnityEngine;

public interface IDamageable
{
    bool CreateDecalsOnHit { get; }

    void OnHit(float damage);
}
