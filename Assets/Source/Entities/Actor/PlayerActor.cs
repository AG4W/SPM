using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerActor : HumanoidActor
{
    [SerializeField]GameObject[] torches;

    [SerializeField]Checkpoint[] checkpoints;

    Transform jig;

    protected override void Initalize()
    {
        base.Initalize();

        checkpoints = FindObjectsOfType<Checkpoint>();
        Debug.Assert(checkpoints != null && checkpoints.Length > 0, "Could not find any checkpoints, did you forget to drag the prefab into your scene?", this.gameObject);
        
        jig = FindObjectOfType<CameraController>().transform;

        if (jig == null)
            Debug.LogError("LocomotionController could not find Camera Jig, did you forget to drag the prefab into your scene?");

        //flytta detta till en separat controller sen, borde nog inte vara här
        //GlobalEvents.Subscribe(GlobalEvent.ToggleTorches, (object[] args) =>
        //{
        //    for (int i = 0; i < torches.Length; i++)
        //        torches[i].SetActive(!torches[i].activeSelf);
        //});
        GlobalEvents.Subscribe(GlobalEvent.SetPlayerWeapon, (object[] args) =>
        {
            base.WeaponController.SetWeapon(args[0] as Weapon);

            this.Raise(ActorEvent.SetActorLeftHandTarget, base.WeaponController.LeftHandIKTarget);
            this.Raise(ActorEvent.SetActorLeftHandWeight, 1f);
        });

        base.Animator.SetBool("isAlert", true);
    }
    protected override StateMachine InitializeStateMachine()
    {
        return new StateMachine(this,
            Resources.LoadAll<State>(base.Path),
            new Dictionary<Type, object>
            {
                [typeof(CameraController)] = FindObjectOfType<CameraController>(),
                [typeof(Animator)] = this.GetComponent<Animator>(),
                [typeof(Actor)] = this,
                [typeof(WeaponController)] = base.WeaponController,
            },
            typeof(IdleState));
    }

    protected override void Update()
    {
        base.Update();
        DispatchInput();
    }
    protected override void OnAnimatorMove()
    {
        base.OnAnimatorMove();
    }
    protected override void OnAnimatorIK(int layerIndex)
    {
        base.OnAnimatorIK(layerIndex);
    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius)), base.CollisionRadius);
    //    Gizmos.DrawSphere(this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius)), base.CollisionRadius);
    //}
    void DispatchInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
            GlobalEvents.Raise(GlobalEvent.ToggleTorches);

        if (Input.GetKey(KeyCode.O))
            GlobalEvents.Raise(GlobalEvent.ModifyCameraTrauma, 1f);
    }

    protected override void CheckOverlap()
    {
        Vector3 pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius));
        Vector3 pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius));

        Vector3 closestPoint;
        Vector3 hitDirection;
        float hitDist;

        bool overlapCheckA = Physics.CheckSphere(pointA, base.CollisionRadius, base.CollisionMask);
        bool overlapCheckB = Physics.CheckSphere(pointB, base.CollisionRadius, base.CollisionMask);

        int counter = 0;
        while (overlapCheckA == true || overlapCheckB == true)
        {
            Collider[] overlapCollidersA = Physics.OverlapSphere(pointA, base.CollisionRadius, base.CollisionMask);
            if (overlapCollidersA.Length > 0)
                for (int i = 0; i < overlapCollidersA.Length; i++)
                {
                    closestPoint = overlapCollidersA[i].ClosestPoint(pointA); // punkt i den överlappande collidern som är närmast centrum på sfären

                    hitDist = Vector3.Distance(pointA, closestPoint);
                    hitDirection = closestPoint - pointA;

                    this.transform.position += -hitDirection.normalized * (base.CollisionRadius - hitDist + base.SkinWidth); // Vi vill flytta oss bakåt: radien på sfären minus distans

                    this.Velocity += this.Velocity.GetNormalForce(-hitDirection.normalized); // Applicera normalkraft 

                    // Uppdatera pointA/B
                    pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius));
                    pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius));
                }

            Collider[] overlapCollidersB = Physics.OverlapSphere(pointB, base.CollisionRadius, base.CollisionMask);
            if (overlapCollidersB.Length > 0)
                for (int i = 0; i < overlapCollidersB.Length; i++)
                {
                    closestPoint = overlapCollidersB[i].ClosestPoint(pointB);

                    hitDist = Vector3.Distance(pointB, closestPoint);
                    hitDirection = closestPoint - pointB;

                    this.transform.position += -hitDirection.normalized * (base.CollisionRadius - hitDist + base.SkinWidth);

                    this.Velocity += this.Velocity.GetNormalForce(-hitDirection.normalized);

                    pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius));
                    pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius));
                }

            // Kolla overlap igen
            overlapCheckA = Physics.CheckSphere(pointA, base.CollisionRadius, base.CollisionMask);
            overlapCheckB = Physics.CheckSphere(pointB, base.CollisionRadius, base.CollisionMask);

            if (counter >= 100)
                break;
            counter++;
        }
    }
    protected override void OnHealthChanged(float change)
    {
        base.OnHealthChanged(change);

        if(change < 0f)
            GlobalEvents.Raise(GlobalEvent.ModifyCameraTrauma, .05f);
    }
    protected override void OnHealthZero()
    {
        //teleportera till nämrsta checkpoint
        this.transform.position = checkpoints.OrderByDescending(t => t.transform.position.DistanceTo(this.transform.position)).First().transform.position + Vector3.up;
        base.Health.Reset();
    }
}
public enum MovementMode
{
    Crouch,
    Walk,
    Jog,
    Sprint
}
public enum Stance
{
    Crouched,
    Standing,
}
public enum AimMode
{
    Default,
    IronSight,
}
