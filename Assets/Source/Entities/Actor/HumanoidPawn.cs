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
    }
    protected override void UpdateTransform()
    {
        base.UpdateTransform();
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
    }
    protected override void OnHealthZero()
    {
        base.OnHealthZero();
    }
}
