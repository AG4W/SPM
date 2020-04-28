using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField]float sensitivityX = 2f;
    [SerializeField]float sensitivityY = 2f;
    [SerializeField]float translationSpeed = .25f;
    [SerializeField]float cameraTranslationSpeed = 5f;
    [Range(0f, 1f)] [SerializeField] float cameraSkinWidth = 0.8f;
    [SerializeField]LayerMask collisionMask;


    [Header("Camera Positions")]
    [SerializeField]Vector3 defaultPosition;
    [SerializeField]Vector3 ironSightPosition;
    [SerializeField]Vector3 jumpPosition;
    [SerializeField]Vector3 sprintPosition;
    [SerializeField]Vector3 fallPosition;
    Vector3[] positions;

    [SerializeField]Vector3 crouchOffset = new Vector3(0f, -.75f, 0f);

    [Header("Clip settings")]
    [SerializeField]float cullDistance = 50f;
    [SerializeField]float lodCullDistance = 2000f;

    [Header("FOV Settings")]
    [SerializeField]float defaultFOV = 60;
    [SerializeField]float ironSightFOV = 30;
    [SerializeField]float jumpFOV = 80;
    [SerializeField]float sprintFOV = 70;
    [SerializeField]float fallFOV = 90;
    float[] fovs;

    [SerializeField]float maxCameraUpAngle;
    [SerializeField]float maxCameraDownAngle;

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

        target = FindObjectOfType<PlayerActor>().FocusPoint.gameObject;

        if (target == null)
            Debug.LogWarning("CameraController couldnt find Player object, did you forget to drag it into your scene?");

        GlobalEvents.Subscribe(GlobalEvent.SetCameraMode, SetMode);
        GlobalEvents.Subscribe(GlobalEvent.ModifyCameraTrauma, ModifyTrauma);
        GlobalEvents.Subscribe(GlobalEvent.ModifyCameraTraumaCapped, ModifyTraumaCapped);
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
            this.transform.position = Vector3.Lerp(this.transform.position, target.transform.position, translationSpeed * Time.deltaTime);
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
        Vector3 desiredPos = positions[(int)mode] + (Input.GetKey(KeyCode.C) ? crouchOffset : Vector3.zero);
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, CorrectCameraDistance(desiredPos), cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));

        camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, cameraRotation, cameraTranslationSpeed * Time.deltaTime);
    }

    //Collision
    Vector3 CorrectCameraDistance(Vector3 desiredPosition)
    {
        Debug.DrawRay(camera.transform.position, camera.transform.right, Color.cyan);
        Debug.DrawRay(camera.transform.position, -camera.transform.right, Color.cyan);
        Debug.DrawRay(camera.transform.position, camera.transform.up, Color.red);
        Debug.DrawRay(camera.transform.position, -camera.transform.up, Color.red);

        RaycastHit hit;
        //Vector3[] directions = new Vector3[] { camera.transform.right, -camera.transform.right, camera.transform.up, -camera.transform.up };
        //for (int i = 0; i < directions.Length; i++)
        //{
        //    while(Physics.Raycast(camera.transform.position+desiredPosition, directions[i], out hit, 0.5f, collisionMask))
        //    {
        //        desiredPosition.z += 0.1f;
        //    }
        //}

        if (Physics.Linecast(this.transform.position, this.transform.position + desiredPosition, out hit, collisionMask))
        {
            if (hit.distance <= (this.transform.position + desiredPosition).magnitude)
            {
                desiredPosition.z = -hit.distance * cameraSkinWidth;
                //return desiredPosition;
            }
        }
        if (Physics.Linecast(this.transform.position, this.transform.position + desiredPosition, out hit, collisionMask))
        {

        }

        return desiredPosition;
    }

    void UpdateFieldOfView() => camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fovs[(int)mode], cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));
    void UpdateDepthOfField()
    {
        Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, Mathf.Infinity);

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
    void ModifyTrauma(object[] args) => trauma = Mathf.Clamp01(trauma + (float)args[0]);
    void ModifyTraumaCapped(object[] args) => trauma = Mathf.Clamp01(trauma + Mathf.Clamp01((float)args[0] - trauma));


}

public enum CameraMode
{
    Default,
    IronSight,
    Jump,
    Sprint,
    Fall,
}