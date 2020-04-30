using UnityEngine;

using System;

public class HumanoidActor : Actor
{
    [Header("Bipedal Settings")]
    [SerializeField]float crouchHeight = 1.4f;

    //vad används denna för
    //och gör dne vad den heter?
    [SerializeField]float groundCheckDistance = .5f;

    [Header("Animation")]
    [SerializeField]AimMode aimMode = AimMode.Default;
    [SerializeField]float actualAimStance;
    [SerializeField]float interpolationSpeed = 2.5f;

    [SerializeField]Stance stance = Stance.Standing;
    [SerializeField]float actualStance;

    float[] targetLayerWeights;
    float[] actualLayerWeights;

    WeaponIndex targetWeaponIndex;
    float actualWeaponIndex;

    [Header("IK")]
    [SerializeField] Vector3 targetLookAtPosition;
    [SerializeField] Vector3 actualLookAtPosition;

    float[] targetLookAtWeights = new float[5];
    float[] actualLookAtWeights = new float[5];

    Transform leftHandTarget;
    float targetLeftHandWeight;
    float actualLeftHandWeight;

    [SerializeField] float lookAtInterpolationSpeed = 1.5f;

    protected override float CurrentHeight => Mathf.Lerp(crouchHeight, base.Height, actualStance);
    protected override float CurrentFeetOffset => base.CurrentFeetOffset;
    protected Animator Animator { get; private set; }

    public bool IsGrounded { get; set; }

    // Använd inte Start/Awake i klasser som ärver ifrån Entity!
    // Använd istället protected override base.Initialize(), denna kallas i Start.
    // Kom ihåg att anropa basmetoden också.
    protected override void Initalize()
    {
        this.Animator = this.GetComponent<Animator>();

        targetLayerWeights = new float[Enum.GetNames(typeof(AnimatorLayer)).Length];
        actualLayerWeights = new float[Enum.GetNames(typeof(AnimatorLayer)).Length];

        //Input
        this.Subscribe(ActorEvent.SetTargetStance, SetTargetStance);
        this.Subscribe(ActorEvent.SetTargetAimMode, SetTargetAimMode);
        this.Subscribe(ActorEvent.SetTargetWeaponIndex, SetTargetWeaponIndex);

        //animator
        this.Subscribe(ActorEvent.SetAnimatorFloat, SetAnimatorFloat);
        this.Subscribe(ActorEvent.SetAnimatorTrigger, SetAnimatorTrigger);
        this.Subscribe(ActorEvent.SetAnimatorBool, SetAnimatorBool);
        this.Subscribe(ActorEvent.SetAnimatorLayer, SetAnimatorLayer);

        //IK
        this.Subscribe(ActorEvent.SetLookAtPosition, SetLookAtPosition);
        this.Subscribe(ActorEvent.SetLookAtWeights, SetLookAtWeights);
        this.Subscribe(ActorEvent.SetLeftHandTarget, SetLeftHandTarget);
        this.Subscribe(ActorEvent.SetLeftHandWeight, SetLeftHandWeight);

        this.Subscribe(ActorEvent.SetWeapon, (object[] args) => {
            this.Raise(ActorEvent.SetTargetWeaponIndex, args[0] == null ? WeaponIndex.Unarmed : ((Weapon)args[0]).Index);
            
        });

        //GroundCheck
        this.Subscribe(ActorEvent.UpdateGroundedStatus, (object[] args) => UpdateGroundedStatus());

        //exekveringsorder är relevant
        //vill regga events innan vi kallar basklassen
        base.Initalize();
    }

    protected override void Update()
    {
        base.Update();

        UpdateAnimator();

        Debug.DrawLine(this.FocusPoint.position, actualLookAtPosition, Color.magenta);
    }
    protected virtual void OnAnimatorMove()
    {
        base.Velocity = new Vector3(this.Animator.velocity.x, base.Velocity.y, this.Animator.velocity.z);
        base.StateMachine.Tick();

        // Vi kollar detta sist så att vi inte råkar förflytta karaktären efter att vi har kollat för kollision
        base.CheckCollision();

        //if (IsGrounded && this.TargetInput.magnitude > .1f)
        //{
        //    Vector3 velocityXZ = this.Velocity;
        //    velocityXZ.y = 0f;

        //    //kolla ifall vi träffar något grundplan, ifall sant, raycasta med vår stepIgnore eller liknande för att se ifall vi kan stiga över det.
        //    if (Physics.Raycast(this.transform.position + (Vector3.up * .1f), velocityXZ.normalized, out RaycastHit botHit, stepOverDistance, base.CollisionMask))
        //    {
        //        if (!Physics.Raycast(this.transform.position + (Vector3.up * actualStepOverHeight), velocityXZ.normalized, out RaycastHit topHit, stepOverDistance, base.CollisionMask))
        //        {
        //            //base.Velocity += Vector3.up * actualStepOverHeight * (mode == MovementMode.Sprint ? 2f : 1f);
        //            this.transform.position += Vector3.up * actualStepOverHeight;

        //            Debug.Log("Ignoring step");
        //        }

        //        Debug.DrawRay(this.transform.position + (Vector3.up * .25f), velocityXZ.normalized * stepOverDistance, topHit.transform == null ? Color.red : Color.green);
        //    }

        //    Debug.DrawRay(this.transform.position + (Vector3.up * .1f), velocityXZ.normalized * stepOverDistance, botHit.transform == null ? Color.red : Color.green);
        //}

        this.transform.position += base.Velocity * (Time.deltaTime / Time.timeScale);
    }
    protected virtual void OnAnimatorIK(int layerIndex)
    {
        //kommer lookatgrejs här sen
        //Look at
        this.Animator.SetLookAtPosition(actualLookAtPosition);
        this.Animator.SetLookAtWeight(actualLookAtWeights[0], actualLookAtWeights[1], actualLookAtWeights[2], actualLookAtWeights[3], actualLookAtWeights[4]);

        //left hand
        if (leftHandTarget == null)
            return;

        this.Animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
        this.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, actualLeftHandWeight);

        this.Animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        this.Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, actualLeftHandWeight);
    }

    protected override void Interpolate()
    {
        base.Interpolate();

        actualStance = Mathf.Lerp(actualStance, (int)stance, interpolationSpeed * (Time.deltaTime / Time.timeScale));
        actualAimStance = Mathf.Lerp(actualAimStance, (int)aimMode, lookAtInterpolationSpeed * (Time.deltaTime / Time.timeScale));
        actualWeaponIndex = Mathf.Lerp(actualWeaponIndex, (int)targetWeaponIndex, interpolationSpeed * (Time.deltaTime / Time.timeScale));

        for (int i = 0; i < targetLayerWeights.Length; i++)
            actualLayerWeights[i] = Mathf.Lerp(actualLayerWeights[i], targetLayerWeights[i], interpolationSpeed * (Time.deltaTime / Time.timeScale));

        //IK
        actualLookAtPosition = Vector3.Lerp(actualLookAtPosition, targetLookAtPosition, lookAtInterpolationSpeed * (Time.deltaTime / Time.timeScale));
        actualLeftHandWeight = Mathf.Lerp(actualLeftHandWeight, targetLeftHandWeight, lookAtInterpolationSpeed * (Time.deltaTime / Time.timeScale));

        for (int i = 0; i < targetLookAtWeights.Length; i++)
            actualLookAtWeights[i] = Mathf.Lerp(actualLookAtWeights[i], targetLookAtWeights[i], lookAtInterpolationSpeed * (Time.deltaTime / Time.timeScale));
    }
    void UpdateAnimator()
    {
        this.Animator.SetFloat("x", this.ActualInput.x);
        this.Animator.SetFloat("z", this.ActualInput.z);

        this.Animator.SetFloat("stance", actualStance);
        this.Animator.SetFloat("aimStance", actualAimStance);
        this.Animator.SetFloat("weaponIndex", actualWeaponIndex);

        this.Animator.SetFloat("targetMagnitude", base.TargetInput.magnitude);
        this.Animator.SetFloat("actualMagnitude", base.ActualInput.magnitude);

        this.Animator.SetBool("isGrounded", this.IsGrounded);

        for (int i = 0; i < actualLayerWeights.Length; i++)
            this.Animator.SetLayerWeight(i, actualLayerWeights[i]);
    }
    protected virtual void UpdateGroundedStatus()
    {
        if (this.Animator.GetBool("isJumping"))
            this.IsGrounded = false;
        else
            this.IsGrounded = Physics.SphereCast(this.transform.position + (Vector3.up * base.CollisionRadius), base.CollisionRadius, Vector3.down, out RaycastHit hit, groundCheckDistance);
    }

    void SetTargetStance(object[] args) => stance = (Stance)args[0];
    void SetTargetAimMode(object[] args) => aimMode = (AimMode)args[0];
    void SetTargetWeaponIndex(object[] args) => targetWeaponIndex = (WeaponIndex)args[0];

    void SetAnimatorFloat(object[] args) => this.Animator.SetFloat((string)args[0], (float)args[1]);
    void SetAnimatorTrigger(object[] args) => this.Animator.SetTrigger((string)args[0]);
    void SetAnimatorBool(object[] args) => this.Animator.SetBool((string)args[0], (bool)args[1]);
    void SetAnimatorLayer(object[] args) => targetLayerWeights[(int)(AnimatorLayer)args[0]] = (float)args[1];

    //IK
    void SetLookAtPosition(object[] args) => targetLookAtPosition = (Vector3)args[0];
    void SetLookAtWeights(object[] args) => targetLookAtWeights = (float[])args[0];

    void SetLeftHandTarget(object[] args) => leftHandTarget = (Transform)args[0];
    void SetLeftHandWeight(object[] args) => targetLeftHandWeight = (float)args[0];
}