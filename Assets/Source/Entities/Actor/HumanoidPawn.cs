using UnityEngine;
using UnityEngine.AI;

using System.Linq;

public class HumanoidPawn : Pawn
{
    [SerializeField]GameObject ragdoll;
    [SerializeField]GameObject model;

    float inputModifier;

    [SerializeField]float preferedCombatDistance = 20f;

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
        this.transform.rotation = Quaternion.LookRotation(base.HasSeenTarget ? base.HeadingToTarget.normalized : (base.DesiredPosition - this.transform.position).normalized, Vector3.up);
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

    protected override void OnDesiredPositionReached()
    {
        base.OnDesiredPositionReached();

        Vector3 newPos;

        if (CanSeeTarget)
        {
            if (base.DistanceToTarget > (preferedCombatDistance * 2))
            {
                base.ActionDebugStatus = "Charging to prefered combat distance.";

                newPos = base.Target.transform.position + (base.HeadingFromTarget.normalized * preferedCombatDistance);
                inputModifier = 2f;
            }
            else
            {
                Vector3 optimalPos = this.transform.position;
                float optimalNormal = 1f;

                for (int i = 0; i < 8; i++)
                {
                    NavMesh.FindClosestEdge(this.transform.position + (Random.insideUnitSphere * (preferedCombatDistance / 2)), out NavMeshHit hit, -1);
                    float d = Vector3.Dot(hit.normal, base.HeadingToTarget);

                    Debug.DrawLine(this.transform.position, hit.position, Color.yellow, 1f);

                    if (d < optimalNormal)
                    {
                        d = optimalNormal;
                        optimalPos = hit.position;
                    }
                }

                base.ActionDebugStatus = "Looking for cover.";

                newPos = optimalPos;
                inputModifier = 1f;
            }
        }
        else
        {
            if (HasSeenTarget)
            {
                base.ActionDebugStatus = "Hunting player.";

                newPos = base.Target.transform.position;
                inputModifier = 1f;
            }
            //ifall vi har hört något
            else if (base.Memory.Count > 0)
            {
                base.ActionDebugStatus = "Investigating noise.";

                newPos = base.Memory[base.Memory.Keys.Min()];
                base.Memory.Remove(base.Memory.Keys.Min());
                inputModifier = 1f;
            }
            else
            {
                //fallbackbeteende är att vandra runt randomly
                base.ActionDebugStatus = "Idling.";

                NavMesh.SamplePosition(this.transform.position + (Random.insideUnitSphere * 10f), out NavMeshHit hit, 10f, -1);
                newPos = hit.position;
                inputModifier = .5f;
            }
        }

        base.SetDesiredPosition(newPos);
        base.TargetInput *= inputModifier;
    }
    protected override void OnNoiseCreated(object[] args)
    {
        base.OnNoiseCreated(args);
        OnDesiredPositionReached();
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
