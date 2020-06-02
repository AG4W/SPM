using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;
using System;

public class PlayerActor : HumanoidActor, IPersistable
{
    [Header("Player Configuration")]
    [SerializeField]float maxForce = 200f;
    [SerializeField]float forceRegenerationAmount = .01f;

    Light[] torches;
    CheckpointData checkpointData;

    //animation setuff
    Transform jig;

    [Header("Player Actor Collision Settings")]
    [Range(.8f, 1f)][SerializeField]float overlapRadiusModifier = 0.9f;

    [Header("Debug Collision")]
    [SerializeField]bool drawCollisionSpheres = false;

    Collider[] overlaps;

    public Vital Force { get; private set; }

    //mhmmm... "unik" hash-generation osv...
    string IPersistable.Hash => "PLAYER()!#¤%&/" +  this.gameObject.activeSelf.ToString(); //this.transform.position.ToString() +
    //player is always persistable, duh
    bool IPersistable.IsPersistable => true;
    bool IPersistable.PersistBetweenScenes => true;

    protected override void Initalize()
    {
        this.Force = new Vital(VitalType.Force, maxForce, forceRegenerationAmount, false);
        this.Force.OnCurrentChanged += OnForceChanged;

        jig = FindObjectOfType<CameraController>().transform;
        Debug.Assert(jig != null, "LocomotionController could not find Camera Jig, did you forget to drag the prefab into your scene?");

        torches = new Light[] {
            this.transform.FindRecursively("torch").GetComponentInChildren<Light>() 
        };

        this.Subscribe(ActorEvent.ShotHit, (object[] args) => GlobalEvents.Raise(GlobalEvent.PlayerShotHit, args));
        this.Subscribe(ActorEvent.ShotMissed, (object[] args) => GlobalEvents.Raise(GlobalEvent.PlayerShotMissed, args));

        //flytta detta till en separat controller sen, borde nog inte vara här
        GlobalEvents.Subscribe(GlobalEvent.ToggleTorches, (object[] args) =>
        {
            for (int i = 0; i < torches.Length; i++)
                torches[i].enabled = !torches[i].enabled;
        });
        GlobalEvents.Subscribe(GlobalEvent.SetPlayerWeapon, (object[] args) => this.Raise(ActorEvent.SetWeapon, args[0]));
        GlobalEvents.Subscribe(GlobalEvent.SetPlayerCurrentCheckpoint, (object[] args) => checkpointData = (CheckpointData)args[0]);

        base.Initalize();
        base.StateMachine.Initialize(this,
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

        this.Force.Tick();
    }
    protected override void OnAnimatorMove()
    {
        base.OnAnimatorMove();
    }
    protected override void OnAnimatorIK(int layerIndex)
    {
        base.OnAnimatorIK(layerIndex);
    }
    private void OnDrawGizmos()
    {
        if (drawCollisionSpheres)
        {
            Gizmos.DrawSphere(this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier)), base.CollisionRadius);
            Gizmos.DrawSphere(this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier)), base.CollisionRadius);
        }
    }
    void DispatchInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
            GlobalEvents.Raise(GlobalEvent.ToggleTorches);
        if (Input.GetKey(KeyCode.O))
            GlobalEvents.Raise(GlobalEvent.ModifyCameraTrauma, 1f);
        if (Input.GetKeyDown(KeyCode.K))
            base.Health.Update(-999999f);
        if (Input.GetKeyDown(KeyCode.U))
        {
            checkpointData = null;
            GlobalEvents.Raise(GlobalEvent.OnSceneExit, SceneManager.GetActiveScene());
            SceneManager.LoadScene(2);
        }
    }

    protected override void CheckOverlap()
    {
        Vector3 pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier));
        Vector3 pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier));

        bool overlapCheckA = Physics.CheckSphere(pointA, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);
        bool overlapCheckB = Physics.CheckSphere(pointB, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);

        int counter = 0;
        while (overlapCheckA == true || overlapCheckB == true)
        {
            CheckOverlapPosition(ref pointA, ref pointA, ref pointB);
            CheckOverlapPosition(ref pointB, ref pointA, ref pointB);

            // Kolla overlap igen
            overlapCheckA = Physics.CheckSphere(pointA, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);
            overlapCheckB = Physics.CheckSphere(pointB, base.CollisionRadius * overlapRadiusModifier, base.CollisionMask);

            if (counter >= 100)
                break;
            counter++;
        }
    }
    private void CheckOverlapPosition(ref Vector3 point, ref Vector3 pointA, ref Vector3 pointB)
    {
        if (Physics.OverlapSphereNonAlloc(point, base.CollisionRadius * overlapRadiusModifier, overlaps, base.CollisionMask) > 0)
        {
            Vector3 closestPoint;
            Vector3 hitDirection;
            float hitDist;
            
            for (int i = 0; i < overlaps.Length; i++)
            {
                closestPoint = overlaps[i].ClosestPoint(point); // punkt i den överlappande collidern som är närmast centrum på sfären

                hitDist = Vector3.Distance(point, closestPoint);
                hitDirection = closestPoint - point;

                this.transform.position += -hitDirection.normalized * (base.CollisionRadius * overlapRadiusModifier - hitDist + base.SkinWidth); // Vi vill flytta oss bakåt: radien på sfären minus distans
                pointA = this.transform.position + (Vector3.up * (base.CurrentHeight - base.CollisionRadius * overlapRadiusModifier));
                pointB = this.transform.position + (Vector3.up * (base.CurrentFeetOffset + base.CollisionRadius * overlapRadiusModifier));

                this.Velocity += this.Velocity.GetNormalForce(-hitDirection.normalized); // Applicera normalkraft 
            }
        }
    }

    protected override void OnHealthChanged(float change)
    {
        base.OnHealthChanged(change);
        GlobalEvents.Raise(GlobalEvent.PlayerHealthChanged, base.Health);
    }
    void OnForceChanged(float change)
    {
        GlobalEvents.Raise(GlobalEvent.PlayerForceChanged, this.Force);
    }
    protected override void OnHealthZero()
    {
        GlobalEvents.Raise(GlobalEvent.OnSceneExit, SceneManager.GetActiveScene());
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void IPersistable.OnEnter(Context context)
    {
        this.checkpointData = (CheckpointData)context.data["checkpointData"];

        if (this.checkpointData == null)
            this.checkpointData = new CheckpointData(this.transform.position, this.transform.rotation);

        this.transform.position = this.checkpointData.Position;
        this.transform.rotation = this.checkpointData.Rotation;

        // NOTE(Krulls): Currently, keeping the weapon on scene transition doesn't work. Respawn on checkpoint keeps the weapon sometimes? verkar bugga... 
        if ((Weapon)context.data["weapon"] == null)
            GlobalEvents.Raise(GlobalEvent.SetPlayerWeapon, this.WeaponController.Weapon);
        else
            GlobalEvents.Raise(GlobalEvent.SetPlayerWeapon, (Weapon)context.data["weapon"]);

        //Debug.Log(context.data["weapon"]);
    }
    Context IPersistable.GetContext()
    {
        Context c = new Context();

        c.data.Add("checkpointData", checkpointData);
        c.data.Add("weapon", this.WeaponController.Weapon);

        return c;
    }
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
