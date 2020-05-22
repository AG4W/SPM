using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField]float sensitivityX = 80f;
    [SerializeField]float sensitivityY = 80f;
    [SerializeField]float jigTranslationSpeed = 15f;
    [SerializeField]float cameraTranslationSpeed = 2.5f;

    [SerializeField]float maxCameraUpAngle = 70;
    [SerializeField]float maxCameraDownAngle = 80;

    [Header("Trauma")]
    [SerializeField]float traumaInterpolationSpeed = 5f;
    float trauma;
    float Shake { get { return trauma * trauma * trauma; } }

    [Header("Collision Settings")]
    [Tooltip("Layers the camera should collide with")]
    [SerializeField]LayerMask collisionMask;

    [Tooltip("The distance which the camera should keep away from colliders")]
    [Range(0f, .5f)][SerializeField]float cameraSkinWidth = .2f;
    [Tooltip("The closest distance to which the camera will go behind the character. " +
        "A small value may push the camera into the head/body, while a big value may keep the camera behind walls (Z-axis)")]
    [Range(0, .4f)][SerializeField]float minDistBehindPlayer = .4f;
    [Tooltip("The closest distance to which the camera will go to the side of the character. " +
        "A small value may push the camera into the head/body, while a big value may keep the camera behind walls (X-axis)")]
    [Range(0, .4f)][SerializeField]float minDistBesidePlayer = .2f;
    [Tooltip("The distance out from the player, left and right, to check to see if the camera should switch shoulder to avoid walls (X-axis)")]
    [Range(.1f, 1f)][SerializeField]float switchShoulderDist = .5f;
    [Tooltip("The translation speed of the camera while it's colliding. (Higher than normal to avoid being pushed behind walls resulting in noticeable culling)")]
    [SerializeField]float cameraTranslationSpeedOnCollision = 5f;

    bool cameraIsColliding = false;

    [Header("Player transparency settings")]
    [Tooltip("The distance of the Camera behind the Player, at which transparancy change kicks in.")]
    [Range(0f, 1f)][SerializeField]float startTransparancyDist = .7f;
    [Tooltip("The distance of the Camera behind the Player, at which full transparancy is applied.")]
    [Range(0f, 1f)][SerializeField]float fullTransparancyDist = .4f;
    /// <summary>
    /// Lazy way to stop redundant SetPlayerAlpha calls
    /// </summary>
    bool playerAlphaIsZero = false;

    [Header("Debug Collision")]
    [Tooltip("Draw gizmos used for debugging (Programmers debugtool).")]
    [SerializeField]bool drawGizmos = false;

    [SerializeField]CameraSettings settings = new CameraSettings(60f, new Vector3(.6f, .4f, -1.1f), new Vector3(0f, 0f, 0f));

    float jigRotationX;
    float jigRotationY;

    new Camera camera;
    GameObject cameraFocusPoint;

    RaycastHit[] hitInfo = new RaycastHit[24];
    RaycastHit[] rightHitInfo = new RaycastHit[24];
    RaycastHit[] leftHitInfo = new RaycastHit[24];

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        camera = this.GetComponentInChildren<Camera>();
        cameraFocusPoint = FindObjectOfType<PlayerActor>().transform.FindRecursively("cameraFocusPoint").gameObject;

        if (cameraFocusPoint == null)
            Debug.LogWarning("CameraController couldn't find Player object, did you forget to drag it into your scene?");

        GlobalEvents.Subscribe(GlobalEvent.SetCameraSettings, SetSettings);
        GlobalEvents.Subscribe(GlobalEvent.ModifyCameraTrauma, ModifyTrauma);
        GlobalEvents.Subscribe(GlobalEvent.ModifyCameraTraumaCapped, ModifyTraumaCapped);
        GlobalEvents.Subscribe(GlobalEvent.PlayerHealthChanged, (object[] args) =>
        {
            Vital v = (Vital)args[0];

            if (v.LatestChange < 0f)
                this.ModifyTrauma(Mathf.Abs(v.LatestChange) / v.Current);
        });
    }
    void LateUpdate()
    {
        UpdateJig();
        UpdateCamera();

        /// Make the player's character transparant when the camera gets too close
        if (camera.transform.localPosition.z >= -fullTransparancyDist && !playerAlphaIsZero)
        {
            GlobalEvents.Raise(GlobalEvent.SetPlayerAlpha, 0f);
            playerAlphaIsZero = true;
        }
        else
        {
            GlobalEvents.Raise(GlobalEvent.SetPlayerAlpha, Mathf.InverseLerp(-fullTransparancyDist, -startTransparancyDist, camera.transform.localPosition.z));
            playerAlphaIsZero = false;
        }
    }

    public void SetSettings(CameraSettings settings) => this.settings = settings;
    public void SetFarClipDistance(float distance) => camera.farClipPlane = distance;
    void SetSettings(object[] args) => SetSettings((CameraSettings)args[0]);

    void UpdateJig()
    {
        jigRotationX += Input.GetAxis("Mouse X") * sensitivityX * (Time.deltaTime / Time.timeScale);
        jigRotationY += -Input.GetAxis("Mouse Y") * sensitivityY * (Time.deltaTime / Time.timeScale);
        jigRotationY = Mathf.Clamp(jigRotationY, -maxCameraUpAngle, maxCameraDownAngle);

        this.transform.rotation = Quaternion.Euler(jigRotationY, jigRotationX, 0f);

        if (cameraFocusPoint != null)
            this.transform.position = Vector3.Lerp(this.transform.position, cameraFocusPoint.transform.position, jigTranslationSpeed * Time.deltaTime);
    }
    void UpdateCamera()
    {
        InterpolateTrauma();
        ApplyCameraShake();

        UpdateCameraPosition();

        UpdateFieldOfView();
        UpdateDepthOfField();
    }

    void UpdateCameraPosition()
    {
        cameraIsColliding = false;
        Vector3 correctedCamPos = CorrectCameraPosition(settings.Position); // cameraIsColliding blir true här vid kollision
        float camTransSpeed = cameraIsColliding == true ? cameraTranslationSpeedOnCollision : cameraTranslationSpeed;

        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, correctedCamPos, camTransSpeed * (Time.deltaTime / Time.timeScale));
        camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, settings.Rotation, camTransSpeed * (Time.deltaTime / Time.timeScale));
    }

    #region Collision
    /// <summary>
    /// Evaluates a good position for the camera. Avoiding collision with the set "collisionMask". IWS = InWorldSpace & ILS = InLocalSpace 
    /// </summary>
    /// <param name="desiredPosILS">The desired position, In Local Space, for the camera.</param>
    /// <returns></returns>
    Vector3 CorrectCameraPosition(Vector3 desiredPosILS)
    {
        Vector3 desiredPosIWS = transform.TransformPoint(desiredPosILS);

        // EvaluateShoulderPosition must be done first
        desiredPosILS = EvaluateShoulderPosition(desiredPosILS);
        desiredPosILS = CheckCollisionZaxis(desiredPosILS, desiredPosIWS);
        desiredPosILS = CheckCollisionXaxis(desiredPosILS, desiredPosIWS);
        desiredPosILS = CheckCollisionYaxis(desiredPosILS);

        return desiredPosILS;

        Vector3 CheckCollisionZaxis(Vector3 desiredPositionILS, Vector3 desiredPositionIWS)
        {
            float desiredZpos = desiredPositionILS.z;
            Vector3 dirToDesiredPosFromJigIWS = this.transform.position.DirectionTo(desiredPositionIWS);
            SetDistancesToInfinite(ref hitInfo);

            if (Physics.SphereCastNonAlloc(this.transform.position, cameraSkinWidth, dirToDesiredPosFromJigIWS.normalized,
                hitInfo, dirToDesiredPosFromJigIWS.magnitude + cameraSkinWidth, collisionMask) > 0)
            {
                cameraIsColliding = true;
                hitInfo.SortByHitDistance();

                float zMove = (dirToDesiredPosFromJigIWS.magnitude + cameraSkinWidth) - hitInfo[0].distance;
                desiredPositionILS.z += zMove;
                desiredPositionILS.z = Mathf.Clamp(desiredPositionILS.z, desiredZpos, -minDistBehindPlayer);
            }

            if (drawGizmos)
                Debug.DrawRay(this.transform.position, dirToDesiredPosFromJigIWS, Color.blue);

            return desiredPositionILS;
        }

        Vector3 CheckCollisionXaxis(Vector3 desiredPositionILS, Vector3 desiredPositionIWS)
        {
            if (desiredPositionILS.x == 0f) // There's no need for X-axis collision
                return desiredPositionILS;

            float desiredXpos = desiredPositionILS.x;
            Vector3 desiredPosOtherShoulderIWS = this.transform.TransformPoint(new Vector3(-desiredPositionILS.x, desiredPositionILS.y, desiredPositionILS.z));

            SetDistancesToInfinite(ref hitInfo);
            int hits = Physics.SphereCastNonAlloc(
                desiredPosOtherShoulderIWS, cameraSkinWidth, (camera.transform.right * desiredXpos).normalized,
                hitInfo, Mathf.Abs(desiredXpos) * 2f + cameraSkinWidth, collisionMask);

            if (hits > 0)
                hitInfo.SortByHitDistance();
            else
                return desiredPositionILS;

            for (int i = 0; i < hits; i++)
            {
                if (hitInfo[i].collider != null && Vector3.Dot(hitInfo[i].normal, hitInfo[i].transform.forward) > 0f) // If dot is negative the hit is most likely behind a face
                {
                    /// Extra casts to make sure the hitInfo[i] is relevant i.e. not behind a wall
                    Physics.Raycast(cameraFocusPoint.transform.position, cameraFocusPoint.transform.position.DirectionTo(hitInfo[i].point),
                        out RaycastHit CFPosRayHit, Mathf.Infinity, collisionMask);
                    Physics.Raycast(desiredPositionIWS, desiredPositionIWS.DirectionTo(hitInfo[i].point),
                        out RaycastHit DesPosRayHit2, Mathf.Infinity, collisionMask);

                    if (CFPosRayHit.point == hitInfo[i].point && DesPosRayHit2.point == hitInfo[i].point && CFPosRayHit.normal == hitInfo[i].normal)
                    {
                        cameraIsColliding = true;

                        float xMove = (Mathf.Abs(desiredXpos) * 2f + cameraSkinWidth) - hitInfo[i].distance;
                        /// A positive desiredPositionILS.x subtracts xMove and a negative adds
                        desiredPositionILS.x = desiredPositionILS.x > 0f ? desiredPositionILS.x -= xMove : desiredPositionILS.x += xMove;
                        break;
                    }

                    if (drawGizmos)
                    {
                        Debug.DrawRay(cameraFocusPoint.transform.position, cameraFocusPoint.transform.position.DirectionTo(hitInfo[i].point), Color.yellow);
                        Debug.DrawRay(desiredPositionIWS, desiredPositionIWS.DirectionTo(hitInfo[i].point), Color.yellow);
                    }
                }
            }
            if (drawGizmos)
                Debug.DrawRay(desiredPosOtherShoulderIWS, (camera.transform.right * desiredXpos).normalized * (Mathf.Abs(desiredXpos) * 2f + cameraSkinWidth), Color.red);

            return desiredPositionILS;
        }

        Vector3 CheckCollisionYaxis(Vector3 desiredPositionILS)
        {
            float desiredYpos = desiredPositionILS.y;
            Vector3 desiredPosZeroYIWS = this.transform.TransformPoint(new Vector3(desiredPositionILS.x, 0f, desiredPositionILS.z));

            if (Physics.Raycast(desiredPosZeroYIWS, camera.transform.up, out RaycastHit hit, desiredYpos + cameraSkinWidth, collisionMask)
                && Vector3.Dot(hit.normal, hit.transform.forward) > 0f) // If dot is negative the hit is most likely behind a face
            {
                desiredPositionILS.y -= (desiredYpos + cameraSkinWidth) - hit.distance;
            }

            if (drawGizmos)
                Debug.DrawRay(desiredPosZeroYIWS, camera.transform.up * (desiredYpos + cameraSkinWidth), Color.green);
            return desiredPositionILS;
        }
    }

    /// <summary>
    /// Evaluates which shoulder the camera should be on. IWS = InWorldSpace & ILS = InLocalSpace 
    /// </summary>
    /// <param name="desiredPositionILS">The desired position, In Local Space, for the camera.</param>
    /// <returns></returns>
    Vector3 EvaluateShoulderPosition(Vector3 desiredPositionILS)
    {
        Vector3 desiredPosYaxisIWS = transform.TransformPoint(new Vector3(0f, desiredPositionILS.y, 0f));

        SetDistancesToInfinite(ref rightHitInfo);
        int rightHit = Physics.SphereCastNonAlloc(desiredPosYaxisIWS, .1f, camera.transform.right, rightHitInfo, switchShoulderDist, collisionMask);
        if (rightHit > 0)
            rightHitInfo.SortByHitDistance();

        SetDistancesToInfinite(ref leftHitInfo);
        int leftHit = Physics.SphereCastNonAlloc(desiredPosYaxisIWS, .1f, -camera.transform.right, leftHitInfo, switchShoulderDist, collisionMask);
        if (leftHit > 0)
            leftHitInfo.SortByHitDistance();

        if (leftHit == 0 & rightHit != 0 & rightHitInfo[0].distance <= switchShoulderDist / 3f || // Left side is free and we hit the right with a certain distance
            leftHit != 0 & rightHit != 0 & leftHitInfo[0].distance > rightHitInfo[0].distance) // OR both sides hit and left have more space
        {
            desiredPositionILS.x *= -1f; // Puts the camera on the left shoulder
        }

        if (drawGizmos)
        {
            Debug.DrawRay(desiredPosYaxisIWS, camera.transform.right * switchShoulderDist, Color.magenta);
            Debug.DrawRay(desiredPosYaxisIWS, -camera.transform.right * switchShoulderDist, Color.magenta);
        }

        return desiredPositionILS;
    }

    /// <summary>
    /// Dummies way of reseting an RaycastHit array. Makes every hit-distance in RaycastHit[] equal to Mathf.Infinity to make them irrelevant.
    /// </summary>
    /// <param name="hitInfo">Array to reset</param>
    void SetDistancesToInfinite(ref RaycastHit[] hitInfo)
    {
        for (int i = 0; i < hitInfo.Length; i++)
            hitInfo[i].distance = Mathf.Infinity;
    }
    #endregion

    // FoV & DoF
    void UpdateFieldOfView() => camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, settings.FoV, cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));
    void UpdateDepthOfField()
    {
        Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, Mathf.Infinity, collisionMask);

        if (hit.transform != null)
            GlobalEvents.Raise(GlobalEvent.UpdateDOFFocusDistance, hit.distance);
    }

    // JOOICE
    void InterpolateTrauma()
    {
        trauma -= traumaInterpolationSpeed * Time.timeScale;
        trauma = Mathf.Clamp01(trauma);
    }
    void ApplyCameraShake()
    {
        float s0 = Random.Range(0f, int.MaxValue);

        Vector3 offset = new Vector3(
                (4f * Shake * Mathf.PerlinNoise(s0, Time.deltaTime)) * Random.Range(-1f, 1f),
                (4f * Shake * Mathf.PerlinNoise(s0 + 1, Time.deltaTime)) * Random.Range(-1f, 1f),
                0f//traumaSqRd * Mathf.PerlinNoise(s0, Time.deltaTime) * Random.Range(-1, 1)
            );

        camera.transform.localRotation = Quaternion.Euler(offset.x, offset.y, offset.z);
    }
    void ModifyTrauma(params object[] args) => trauma = Mathf.Clamp01(trauma + (float)args[0]);
    void ModifyTraumaCapped(params object[] args) => trauma = Mathf.Clamp01(trauma + Mathf.Clamp01((float)args[0] - trauma));
}