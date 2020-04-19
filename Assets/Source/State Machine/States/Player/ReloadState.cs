using UnityEngine;

[CreateAssetMenu(menuName = "State/Player/Reload")]
public class ReloadState : ActState
{
    public override void Enter()
    {
        base.Enter();

        base.Actor.Raise(ActorEvent.SetActorAnimatorLayer, AnimatorLayer.Reload, 1f);
        base.Actor.Raise(ActorEvent.SetActorAnimatorTrigger, "reload");

        base.Actor.Raise(ActorEvent.SetActorLeftHandWeight, 0f);

        ((WeaponController)base.Context["weapon"]).Reload();
    }
    public override void Tick()
    {
        base.Tick();

        //kolla reloadtimer
        //ifall vi bailar utan att completa reload pga roll/hopp så yeetar vi
    }
    public override void Exit()
    {
        base.Actor.Raise(ActorEvent.SetActorAnimatorLayer, AnimatorLayer.Reload, 1f);
        base.Actor.Raise(ActorEvent.SetActorLeftHandWeight, 1f);
    }
}
