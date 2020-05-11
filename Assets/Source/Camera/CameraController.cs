/* 
 * This class handles the Camera.
 * Must be assigned to the Camera Jig
 */

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField]float sensitivityX = 80f;
    [SerializeField]float sensitivityY = 80f;
    [SerializeField]float jigTranslationSpeed = 15f;
    [SerializeField]float cameraTranslationSpeed = 2.5f;

    [Header("Collision Settings")]
    [Tooltip("Layers the camera should collide with")]
    [SerializeField]LayerMask collisionMask;

    [Tooltip("The distance which the camera should keep away from colliders")]
    [Range(0f, .5f)][SerializeField] float cameraSkinWidth = .2f;

    [Tooltip("The closest distance to which the camera will go behind the character. " +
        "A small value may push the camera into the head/body, while a big value may keep the camera behind walls (Z-axis)")]
    [Range(0, .4f)][SerializeField]float minDistBehindPlayer = .4f;

    [Tooltip("The closest distance to which the camera will go to the side of the character." +
        "A small value may push the camera into the head/body, while a big value may keep the camera behind walls (X-axis)")]
    [Range(0, .4f)] [SerializeField] float minDistBesidePlayer = .2f;

    [Tooltip("The distance out from the player, left and right, to check to see if the camera should switch shoulder to avoid walls (X-axis)")]
    [Range(.1f, 1f)] [SerializeField] float switchShoulderDist = .5f;

    [Tooltip("The speed which the camera will move during collision. Should be higher than normal")]
    [SerializeField]float camCollisionTransSpeed = 5f; // (Krulls) Not used right now since the shit isn't fucking working

    bool cameraIsColliding = false; // (Krulls) used by the collisionhandler to switch between cameraTranslationSpeed/camCollisionTransSpeed, Not used right now since the shit isn't fucking working

    [Header("Debug Collision")]
    [SerializeField]bool drawGizmos = false;

    [Header("Camera Positions")]
    [SerializeField]Vector3 defaultPosition = new Vector3(.6f, .4f, -1.1f);
    [SerializeField]Vector3 ironSightPosition = new Vector3(.5f, .45f, -.75f);
    [SerializeField]Vector3 jumpPosition = new Vector3(.3f, .5f, -1.4f);
    [SerializeField]Vector3 sprintPosition = new Vector3(.3f, .5f, -1.4f);
    [SerializeField]Vector3 fallPosition = new Vector3(0f, .6f, -4f);
    Vector3[] positions;

    [SerializeField]Vector3 crouchOffset = new Vector3(0f, -.4f, 0f);

    [Header("Clip settings")]
    [SerializeField]float cullDistance = 50f;

    [Header("FOV Settings")]
    [SerializeField]float defaultFOV = 60;
    [SerializeField]float ironSightFOV = 40;
    [SerializeField]float jumpFOV = 70;
    [SerializeField]float sprintFOV = 65;
    [SerializeField]float fallFOV = 90;
    float[] fovs;

    [SerializeField]float maxCameraUpAngle = 70;
    [SerializeField]float maxCameraDownAngle = 80;

    float jigRotationX;
    float jigRotationY;

    [Header("Trauma / Camera shake")]
    float trauma;
    float Shake { get { return trauma * trauma * trauma; } }
    [SerializeField]float traumaInterpolationSpeed = 5f;

    Quaternion cameraRotation;

    Camera camera;
    GameObject cameraFocusPoint;

    CameraMode mode = CameraMode.Default;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        positions = new Vector3[] { defaultPosition, ironSightPosition, jumpPosition, sprintPosition, fallPosition };
        fovs = new float[] { defaultFOV, ironSightFOV, jumpFOV, sprintFOV, fallFOV };

        camera = this.GetComponentInChildren<Camera>();
        cameraRotation = camera.transform.localRotation;

        // Setup camera stuff
        float[] cullingDistances = new float[32];
        for (int i = 0; i < cullingDistances.Length; i++)
            cullingDistances[i] = i == 31 ? 0f : cullDistance;

        camera.layerCullDistances = cullingDistances;
        camera.layerCullSpherical = true;

        cameraFocusPoint = FindObjectOfType<PlayerActor>().transform.FindRecursively("cameraFocusPoint").gameObject;
        if (cameraFocusPoint == null)
            Debug.LogWarning("CameraController couldnt find Player object, did you forget to drag it into your scene?");

        GlobalEvents.Subscribe(GlobalEvent.SetCameraMode, SetMode);
        GlobalEvents.Subscribe(GlobalEvent.ModifyCameraTrauma, ModifyTrauma);
        GlobalEvents.Subscribe(GlobalEvent.ModifyCameraTraumaCapped, ModifyTraumaCapped);
        GlobalEvents.Subscribe(GlobalEvent.PlayerHealthChanged, (object[] args) =>
        {
            Vital v = (Vital)args[0];

            if(v.LatestChange < 0f)
                this.ModifyTrauma(Mathf.Abs(v.LatestChange) / v.Current);
        });
    }
    void LateUpdate()
    {
        UpdateCamera();
        UpdateJig();
    }

    void SetMode(object[] args) => mode = (CameraMode)args[0];

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
        //Vector3 desiredPos = positions[(int)mode] + (Input.GetKey(KeyCode.C) ? crouchOffset : Vector3.zero);
        
        Vector3 desiredPos = GetDesiredPosition();
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, CorrectCameraPosition(desiredPos), cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));
        camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, cameraRotation, cameraTranslationSpeed * Time.deltaTime);
    }
    Vector3 GetDesiredPosition()
    {
        // IWS = InWorldSpace / ILS = InLocalSpace

        // Get player input 
        Vector3 desiredPositionILS = positions[(int)mode];
        if (Input.GetKey(KeyCode.C))
            desiredPositionILS += crouchOffset;

        #region Check which shoulder is best for the camera
        Vector3 desiredPosYaxisIWS = transform.TransformPoint(new Vector3(0f, desiredPositionILS.y, 0f));

        if (drawGizmos)
        {
            Debug.DrawRay(desiredPosYaxisIWS, camera.transform.right * switchShoulderDist, Color.red);
            Debug.DrawRay(desiredPosYaxisIWS, -camera.transform.right * switchShoulderDist, Color.red);
        }

        Physics.SphereCast(desiredPosYaxisIWS, cameraSkinWidth, camera.transform.right, out RaycastHit rightHit, switchShoulderDist, collisionMask);
        Physics.SphereCast(desiredPosYaxisIWS, cameraSkinWidth, -camera.transform.right, out RaycastHit leftHit, switchShoulderDist, collisionMask);
        if (rightHit.transform != null & leftHit.transform == null || rightHit.transform != null & leftHit.transform != null & leftHit.distance > rightHit.distance)
        {
            desiredPositionILS.x *= -1f; // Puts the camera on the left shoulder
        }
        #endregion

        return desiredPositionILS;
    }

    /// <summary>
    /// Takes the desired position, In Local Space, checks for collisions and returns a position, In Local Space, that avoids collision.
    /// </summary>
    /// <param name="desiredPositionILS">The desired position In Local Space.</param>
    /// <returns></returns>
    Vector3 CorrectCameraPosition(Vector3 desiredPositionILS)
    {
        RaycastHit hit;
        float zMove = 0f;
        float desiredZpos = desiredPositionILS.z;
        float desiredXpos = desiredPositionILS.x;

        // ILS = In Local Space, IWS = In World Space
        Vector3 desiredPositionIWS = transform.TransformPoint(desiredPositionILS);
        Vector3 dirToDesiredPosFromJigIWS = this.transform.position.DirectionTo(desiredPositionIWS);

        if (drawGizmos)
            Debug.DrawRay(this.transform.position, dirToDesiredPosFromJigIWS, Color.blue);

        #region Moves the camera in Z-axis at collision
        if (Physics.SphereCast(this.transform.position, cameraSkinWidth, dirToDesiredPosFromJigIWS, out hit, dirToDesiredPosFromJigIWS.magnitude, collisionMask))
        {
            cameraIsColliding = true;
            zMove = (dirToDesiredPosFromJigIWS.magnitude + cameraSkinWidth) - hit.distance;
        }

        desiredPositionILS.z += zMove;
        desiredPositionILS.z = Mathf.Clamp(desiredPositionILS.z, desiredZpos, -minDistBehindPlayer);
        #endregion

        #region Moves in X-axis at collision
        Vector3 desiredPosZeroXIWS = transform.TransformPoint(new Vector3(0f, desiredPositionILS.y, desiredPositionILS.z));

        if (drawGizmos)
            Debug.DrawRay(desiredPosZeroXIWS, camera.transform.right * desiredXpos, Color.red);

        if (Physics.SphereCast(desiredPosZeroXIWS, cameraSkinWidth, camera.transform.right * desiredXpos, out hit, Mathf.Abs(desiredXpos), collisionMask) & desiredXpos != 0f)
        {
            cameraIsColliding = true;

            float xMove;
            if (desiredXpos > 0f) // positive = right side
            {
                xMove = (desiredXpos + cameraSkinWidth) - hit.distance; // xMove becomes positive which we subtract from the desired X-pos
                xMove = Mathf.Clamp(xMove, 0f, desiredXpos);

                desiredPositionILS.x -= xMove;
                desiredPositionILS.x = Mathf.Clamp(desiredPositionILS.x, minDistBesidePlayer, desiredXpos);
            }
            if (desiredXpos < 0f) // negative = left side
            {
                xMove = (desiredXpos - cameraSkinWidth) + hit.distance; // xMove becomes negative which we subtract from the desired -X-pos
                xMove = Mathf.Clamp(xMove, desiredXpos, 0f);

                desiredPositionILS.x -= xMove;
                desiredPositionILS.x = Mathf.Clamp(desiredPositionILS.x, desiredXpos, -minDistBesidePlayer);
            }
        } 
        #endregion

        return desiredPositionILS;
    }

    // FOV
    void UpdateFieldOfView() => camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fovs[(int)mode], cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));
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

public enum CameraMode
{
    Default,
    IronSight,
    Jump,
    Sprint,
    Fall,
}