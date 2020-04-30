﻿using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField]float sensitivityX = 80f;
    [SerializeField]float sensitivityY = 80f;
    [SerializeField]float jigTranslationSpeed = 15f;
    [SerializeField]float cameraTranslationSpeed = 2.5f;

    float currentCameraTranslationSpeed; // used by the collisionhandler to switch between cameraTranslationSpeed/cameraCollisionTranslationSpeed

    [Header("Collision Settings")]
    [Tooltip("Layers the camera should collide with")]
    [SerializeField]LayerMask collisionMask;

    [Tooltip("The distance which the camera should keep away from colliders")]
    [Range(0f, 1f)][SerializeField] float cameraSkinWidth = 0.2f;

    [Tooltip("The closest distance to which the camera will go behind the character")]
    [Range(0, .5f)][SerializeField]float minDistanceBehindCharacter = .4f;

    [Tooltip("The speed which the camera will move during collision. Should be higher than normal")]
    [SerializeField]float cameraCollisionTranslationSpeed = 25f;

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
    [SerializeField]float lodCullDistance = 2000f;

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

    [Header("Trauma")]
    float trauma;
    float shake { get { return trauma * trauma * trauma; } }
    [SerializeField]float traumaInterpolationSpeed = 5f;
    Quaternion cameraRotation;

    Camera camera;
    GameObject target;

    CameraMode mode = CameraMode.Default;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        positions = new Vector3[] { defaultPosition, ironSightPosition, jumpPosition, sprintPosition, fallPosition };
        fovs = new float[] { defaultFOV, ironSightFOV, jumpFOV, sprintFOV, fallFOV };

        camera = this.GetComponentInChildren<Camera>();
        cameraRotation = camera.transform.localRotation;

        //setup camera stuff
        float[] cullingDistances = new float[32];

        for (int i = 0; i < cullingDistances.Length; i++)
            cullingDistances[i] = i == 31 ? 0f : cullDistance;

        camera.layerCullDistances = cullingDistances;
        camera.layerCullSpherical = true;

        target = FindObjectOfType<PlayerActor>().transform.FindRecursively("cameraFocusPoint").gameObject;

        if (target == null)
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
    void Update()
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

        if (target != null)
            this.transform.position = Vector3.Lerp(this.transform.position, target.transform.position, jigTranslationSpeed * Time.deltaTime);
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
        currentCameraTranslationSpeed = cameraTranslationSpeed;

        Vector3 desiredPos = positions[(int)mode] + (Input.GetKey(KeyCode.C) ? crouchOffset : Vector3.zero);
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, CorrectCameraPosition(desiredPos), currentCameraTranslationSpeed * (Time.deltaTime / Time.timeScale));

        camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, cameraRotation, currentCameraTranslationSpeed * Time.deltaTime);
    }

    //Collision
    Vector3 CorrectCameraPosition(Vector3 desiredPositionILS) // ILS = In Local Space, IWS = In World Space
    {
        //Debug.DrawRay(camera.transform.position, camera.transform.right, Color.cyan);
        //Debug.DrawRay(camera.transform.position, -camera.transform.right, Color.cyan);
        //Debug.DrawRay(camera.transform.position, camera.transform.up, Color.red);
        //Debug.DrawRay(camera.transform.position, -camera.transform.up, Color.red);

        //Vector3[] directions = new Vector3[] { camera.transform.right, -camera.transform.right, camera.transform.up, -camera.transform.up };
        //for (int i = 0; i < directions.Length; i++)
        //{
        //    while(Physics.Raycast(camera.transform.position+desiredPosition, directions[i], out hit, 0.5f, collisionMask))
        //    {
        //        desiredPosition.z += 0.1f;
        //    }
        //}

        RaycastHit hit;
        float desiredZ = desiredPositionILS.z;
        Vector3 desiredPositionIWS = transform.TransformPoint(desiredPositionILS);
        Vector3 directionToDesiredPosIWS = this.transform.position.DirectionTo(desiredPositionIWS);
        Vector3 lineCastEndPos = this.transform.position + directionToDesiredPosIWS.normalized * (directionToDesiredPosIWS.magnitude + cameraSkinWidth);

        Debug.DrawRay(camera.transform.position, camera.transform.right * .5f, Color.cyan);
        Debug.DrawRay(camera.transform.position, -camera.transform.right * .5f, Color.cyan);
        //Debug.DrawLine(this.transform.position, desiredPositionIWS, Color.blue); // Rätt
        //Debug.DrawRay(this.transform.position, directionToDesiredPosIWS, Color.black); // Rätt
        Debug.DrawLine(this.transform.position, lineCastEndPos, Color.blue); // Rätt

        //Debug.DrawLine(this.transform.position, (this.transform.position - desiredPosition), Color.blue); // Blir fel..
        // this.transform.position + desiredPosition = kamerans position i worldspace
        if (Physics.Linecast(this.transform.position, lineCastEndPos, out hit, collisionMask))// && hit.distance <= directionToDesiredPos.magnitude
        {
            currentCameraTranslationSpeed = cameraCollisionTranslationSpeed;

            desiredPositionIWS = new Vector3(hit.point.x + hit.normal.x * cameraSkinWidth, desiredPositionIWS.y, hit.point.z + hit.normal.z * cameraSkinWidth);
            float zMove = directionToDesiredPosIWS.magnitude - hit.distance;
            desiredPositionILS.z += zMove;

        } // (KRULLS) Måste ändra sen så att dessa inte är else. Utan else skakar kameran mellan höger/vänster
        if (Physics.Raycast(camera.transform.position, camera.transform.right, out hit, .5f, collisionMask) && hit.distance < 2 * cameraSkinWidth)
        {
            //Debug.Log("Kamera krockar till Höger");
            currentCameraTranslationSpeed = cameraCollisionTranslationSpeed;
            desiredPositionIWS += new Vector3(hit.normal.x * (2 * cameraSkinWidth - hit.distance), 0f, hit.normal.z * (2 * cameraSkinWidth - hit.distance));

        }
        else if (Physics.Raycast(camera.transform.position, -camera.transform.right, out hit, .5f, collisionMask) && hit.distance < 2 * cameraSkinWidth)
        {
            //Debug.Log("Kamera krockar till Vänster");
            currentCameraTranslationSpeed = cameraCollisionTranslationSpeed;
            desiredPositionIWS += new Vector3(hit.normal.x * (2 * cameraSkinWidth - hit.distance), 0f, hit.normal.z * (2 * cameraSkinWidth - hit.distance));

        }

        desiredPositionILS = transform.InverseTransformPoint(desiredPositionIWS);
        desiredPositionILS.z = Mathf.Clamp(desiredPositionILS.z, desiredZ, -minDistanceBehindCharacter);

        return desiredPositionILS;
    }

    void UpdateFieldOfView() => camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fovs[(int)mode], currentCameraTranslationSpeed * (Time.deltaTime / Time.timeScale));
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
                (4f * shake * Mathf.PerlinNoise(s0, Time.deltaTime)) * Random.Range(-1f, 1f),
                (4f * shake * Mathf.PerlinNoise(s0 + 1, Time.deltaTime)) * Random.Range(-1f, 1f),
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