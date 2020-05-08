using UnityEngine;

public class NeutralActor : MonoBehaviour, IDamageable
{
    [SerializeField]NeutralActorAnimation pose;

    IDamageableType IDamageable.Type => IDamageableType.Organic;

    void Start()
    {
        this.GetComponent<Animator>().SetFloat("animationIndex", (int)pose);
    }

    void IDamageable.OnHit(float damage)
    {
        Destroy(this.gameObject);
    }
}
public enum NeutralActorAnimation
{
    Kneeling,
}