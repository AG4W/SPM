using UnityEngine;

public class HumanoidPawn : Pawn
{
    [SerializeField]GameObject ragdoll;
    [SerializeField]GameObject model;

    protected override void Initalize()
    {
        base.Initalize();

        base.Agent.updatePosition = false;
        base.Agent.updateRotation = false;
    }

    void OnAnimatorMove()
    {
        base.Velocity = base.Animator.deltaPosition;
    }

    protected override void UpdateRotation()
    {
        base.UpdateRotation();

        if (base.CanSeeTarget)
        {
            this.transform.LookAt(base.Target.transform, Vector3.up);
        }
    }
    protected override void UpdateAnimator()
    {
        base.UpdateAnimator();

        base.Animator.SetFloat("x", base.ActualInput.x);
        base.Animator.SetFloat("z", base.ActualInput.z);
        base.Animator.SetFloat("inputMagnitude", base.ActualInput.magnitude);

        base.Animator.SetFloat("actualStance", 1f);

        base.Animator.SetBool("isGrounded", true);
    }

    protected override void UpdateTargetStatus()
    {
        base.UpdateTargetStatus();
    }
    protected override void UpdateDestination()
    {
        base.UpdateDestination();
        base.Agent.nextPosition = this.transform.position;

        if (CanSeeTarget)
        {
            base.DesiredPosition = base.Target.transform.position + (-base.HeadingToTarget.normalized * 10f);
            base.TargetInput = (base.DesiredPosition - this.transform.position).normalized;
        }
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
        model.SetActive(false);
        ragdoll.SetActive(true);
    }
}
