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
    [Tooltip("The translation speed of the camera while it's colliding. (Higher than normal to avoid being pushed behind walls)")]
    [SerializeField]float cameraTranslationSpeedOnCollision = 5f;
    bool cameraIsColliding = false;

    [Header("Player transparency settings")]
    [Tooltip("The distance of the Camera behind the Player, at which transparancy change kicks in.")]
    [Range(0f, 1f)][SerializeField]float startTransparancyDist = .7f;
    [Tooltip("The distance of the Camera behind the Player, at which full transparancy is applied.")]
    [Range(0f, 1f)][SerializeField]float fullTransparancyDist = .4f;

    [Header("Debug Collision")]
    [SerializeField]bool drawGizmos = false;

    [SerializeField] CameraSettings settings;

    float jigRotationX;
    float jigRotationY;

    new Camera camera;
    GameObject cameraFocusPoint;
    RaycastHit[] hitInfo = new RaycastHit[24];
    RaycastHit[] rightHitInfo = new RaycastHit[24];

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        camera = this.GetComponentInChildren<Camera>();
        cameraFocusPoint = FindObjectOfType<PlayerActor>().transform.FindRecursively("cameraFocusPoint").gameObject;

        if (cameraFocusPoint == null)
            Debug.LogWarning("CameraController couldnt find Player object, did you forget to drag it into your scene?");

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
        if (camera.transform.localPosition.z >= -fullTransparancyDist)
            GlobalEvents.Raise(GlobalEvent.SetPlayerAlpha, 0f);
        else
            GlobalEvents.Raise(GlobalEvent.SetPlayerAlpha, Mathf.InverseLerp(-fullTransparancyDist, -startTransparancyDist, camera.transform.localPosition.z));
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
    Vector3 CorrectCameraPosition(Vector3 desiredPositionILS)
    {
        //// ILS = In Local Space & IWS = In World Space

        desiredPositionILS = CheckShoulderPosition(desiredPositionILS);

        
        float zMove = 0f;
        float desiredZpos = desiredPositionILS.z;
        float desiredXpos = desiredPositionILS.x;
        float desiredYpos = desiredPositionILS.y;

        Vector3 desiredPositionIWS = transform.TransformPoint(desiredPositionILS);
        Vector3 dirToDesiredPosFromJigIWS = this.transform.position.DirectionTo(desiredPositionIWS);

        #region Moves in Z-axis on collision
        if (drawGizmos)
            Debug.DrawRay(this.transform.position, dirToDesiredPosFromJigIWS, Color.blue);

        if (Physics.SphereCastNonAlloc(this.transform.position, cameraSkinWidth, dirToDesiredPosFromJigIWS.normalized, hitInfo, dirToDesiredPosFromJigIWS.magnitude + cameraSkinWidth, collisionMask) > 0)
        {
            hitInfo.SortByHitDistance();
            cameraIsColliding = true;
            zMove = (dirToDesiredPosFromJigIWS.magnitude + cameraSkinWidth) - hitInfo[0].distance;
        }

        desiredPositionILS.z += zMove;
        desiredPositionILS.z = Mathf.Clamp(desiredPositionILS.z, desiredZpos, -minDistBehindPlayer);
        #endregion

        #region Moves in X-axis on collision 
        if (desiredXpos != 0f)
        {
            //Vector3 desiredPosZeroXIWS = this.transform.TransformPoint(new Vector3(0f, desiredPositionILS.y, desiredPositionILS.z));
            Vector3 desiredPosTest = this.transform.TransformPoint(new Vector3(-desiredPositionILS.x, desiredPositionILS.y, desiredPositionILS.z));

            if (drawGizmos)
                Debug.DrawRay(desiredPosTest, camera.transform.right * desiredXpos * 2f, Color.red);

            if (Physics.SphereCastNonAlloc(desiredPosTest, cameraSkinWidth, (camera.transform.right * desiredXpos).normalized, hitInfo, Mathf.Abs(desiredXpos) * 2f + cameraSkinWidth, collisionMask) > 0
                && Vector3.Dot(hitInfo[0].normal, hitInfo[0].transform.forward) > 0f)
            {
                hitInfo.SortByHitDistance();
                cameraIsColliding = true;
                float xMove;
                if (desiredXpos > 0f) // positive = right shoulder
                {
                    xMove = (desiredXpos * 2f + cameraSkinWidth) - hitInfo[0].distance; // xMove becomes positive which we subtract from the desired X-pos
                    xMove = Mathf.Clamp(xMove, 0f, desiredXpos);

                    desiredPositionILS.x -= xMove;
                    desiredPositionILS.x = Mathf.Clamp(desiredPositionILS.x, minDistBesidePlayer, desiredXpos);
                }
                if (desiredXpos < 0f) // negative = left shoulder
                {
                    xMove = (desiredXpos * 2f - cameraSkinWidth) + hitInfo[0].distance; // xMove becomes negative which we subtract from the desired -X-pos
                    xMove = Mathf.Clamp(xMove, desiredXpos, 0f);

                    desiredPositionILS.x -= xMove;
                    desiredPositionILS.x = Mathf.Clamp(desiredPositionILS.x, desiredXpos, -minDistBesidePlayer);
                }
            }
        }
        #endregion

        // Handle collision above
        Vector3 desiredPosZeroYIWS = this.transform.TransformPoint(new Vector3(desiredPositionILS.x, 0f, desiredPositionILS.z));
        if (drawGizmos)
            Debug.DrawRay(desiredPosZeroYIWS, camera.transform.up * (desiredYpos + cameraSkinWidth), Color.green);

        if (Physics.Raycast(desiredPosZeroYIWS, camera.transform.up, out RaycastHit hit, desiredYpos + cameraSkinWidth, collisionMask)
            && Vector3.Dot(hit.normal, hit.transform.forward) > 0f)
        {
            desiredPositionILS.y -= (desiredYpos + cameraSkinWidth) - hit.distance;
        }

        return desiredPositionILS;
    }
    Vector3 CheckShoulderPosition(Vector3 desiredPositionILS)
    {
        //// IWS = InWorldSpace & ILS = InLocalSpace 
        Vector3 desiredPosYaxisIWS = transform.TransformPoint(new Vector3(0f, desiredPositionILS.y, 0f));

        if (drawGizmos)
        {
            Debug.DrawRay(desiredPosYaxisIWS, camera.transform.right * switchShoulderDist, Color.magenta);
            Debug.DrawRay(desiredPosYaxisIWS, -camera.transform.right * switchShoulderDist, Color.magenta);
        }

        int rightHit = Physics.SphereCastNonAlloc(desiredPosYaxisIWS, .1f, camera.transform.right, rightHitInfo, switchShoulderDist, collisionMask);
        if (rightHit > 0)
            rightHitInfo.SortByHitDistance();

        int leftHit = Physics.SphereCastNonAlloc(desiredPosYaxisIWS, .1f, -camera.transform.right, hitInfo, switchShoulderDist, collisionMask);
        if (leftHit > 0)
            hitInfo.SortByHitDistance();

        if (leftHit == 0 & rightHit != 0 & rightHitInfo[0].distance <= switchShoulderDist / 3f || // Om vänster är fri och vi träffar höger med viss distans
            leftHit != 0 & rightHit != 0 & hitInfo[0].distance > rightHitInfo[0].distance) // Träffar båda sidor och vänster har mer plats
        {
            desiredPositionILS.x *= -1f; // Puts the camera on the left shoulder
        }
        //if (Input.GetKey(KeyCode.Q))
        //    desiredPositionILS.x *= -1f;

        return desiredPositionILS;
    }
    #endregion

    void UpdateFieldOfView() => camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, settings.FoV, cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));
    void UpdateDepthOfField()
    {
        Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, Mathf.Infinity, collisionMask);

        if (hit.transform != null)
            GlobalEvents.Raise(GlobalEvent.UpdateDOFFocusDistance, hit.distance);
    }

    //JOOICE
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