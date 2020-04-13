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

        base.DesiredPosition = this.transform.position;
    }

    void OnAnimatorMove()
    {
        Debug.DrawLine(this.transform.position, base.Agent.destination, Color.blue);
        base.Velocity = base.Animator.deltaPosition;
    }

    protected override void UpdateRotation()
    {
        base.UpdateRotation();
        this.transform.LookAt(CanSeeTarget ? base.Target.transform.position : base.DesiredPosition, Vector3.up);
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

    //detta händer konstant vid varje refresh
    //ett bättre sätt är att göra det rekursivt när AI'n är complete
    //så att AIn går mot ett mål, och först när vi når målet så kallar vi denna metod igen
    protected override void UpdateDestination()
    {
        base.UpdateDestination();

        if (CanSeeTarget)
        {
            if(base.DistanceToTarget > (preferedCombatDistance * 2))
            {
                base.DesiredPosition = base.Target.transform.position + (base.HeadingFromTarget.normalized * preferedCombatDistance);
                inputModifier = 2f;

                base.DebugText += "\nCharging to prefered combat distance.";
            }
            else
            {
                Vector3 optimalPos = this.transform.position + (Random.insideUnitSphere * (preferedCombatDistance / 2));
                float optimalNormal = 1f;

                for (int i = 0; i < 5; i++)
                {
                    NavMesh.FindClosestEdge(this.transform.position + (Random.insideUnitSphere * (preferedCombatDistance / 2)), out NavMeshHit hit, -1);
                    float d = Vector3.Dot(hit.normal, base.HeadingToTarget);

                    if(d < optimalNormal)
                    {
                        d = optimalNormal;
                        optimalPos = hit.position;
                    }
                }

                base.DebugText += "\nLooking for cover.";
                base.DesiredPosition = optimalPos;
                inputModifier = 1f;
            }
        }
        else
        {
            if (Vector3.Distance(base.DesiredPosition, this.transform.position) < .5f)
            {
                Vector3 newDesiredPosition;

                //ifall vi har hört något
                if (base.Memory.Count > 0)
                {
                    base.DebugText += "\nInvestigating noise.";
                    newDesiredPosition = base.Memory[base.Memory.Keys.Min()];
                    base.Memory.Remove(base.Memory.Keys.Min());
                    inputModifier = 1f;
                }
                else
                {
                    //fallbackbeteende är att vandra runt randomly
                    base.DebugText += "\nIdling.";
                    NavMesh.SamplePosition(this.transform.position + (Random.insideUnitSphere * 10f), out NavMeshHit hit, 10f, -1);
                    newDesiredPosition = hit.position;
                    inputModifier = .5f;
                }

                base.DesiredPosition = newDesiredPosition;
            }
            else
                base.DebugText += "\nMoving to new position.";
        }

        base.Agent.SetDestination(base.DesiredPosition);
        base.TargetInput = base.Agent.nextPosition.ToInput(this.transform).normalized;
        base.TargetInput *= inputModifier;
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
