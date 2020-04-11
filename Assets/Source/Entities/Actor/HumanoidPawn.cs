using UnityEngine;

public class HumanoidPawn : Pawn
{
    protected override void Initalize()
    {
        base.Initalize();
    }
    
    protected override void UpdateVelocity()
    {
        base.UpdateVelocity();
    }
    protected override void UpdateRotation()
    {
        base.UpdateRotation();

        if (base.HasSeenTarget)
            this.transform.LookAt(base.Target.transform, Vector3.up);
    }
    protected override void UpdateAnimator()
    {
        base.UpdateAnimator();
    }

    protected override void UpdateDestination()
    {
        base.UpdateDestination();
    }
    protected override void UpdateTarget()
    {
        base.UpdateTarget();
    }

    protected override void OnHealthChanged(float current)
    {
        base.OnHealthChanged(current);

        //todo, add damage sounds etc
        //hit reactions in animation?
    }
    protected override void OnHealthZero()
    {
        base.OnHealthZero();

        //add ragdoll
    }
}
