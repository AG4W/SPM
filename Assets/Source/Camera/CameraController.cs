using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CameraController : MonoBehaviour
{
    [SerializeField]VolumeProfile profile;
    DepthOfField dof;

    [SerializeField]float sensitivityX = 2f;
    [SerializeField]float sensitivityY = 2f;
    [SerializeField]float translationSpeed = 5f;

    [SerializeField]float cameraTranslationSpeed = 5f;

    [SerializeField]Vector3 defaultPosition;
    [SerializeField]Vector3 ironSightPosition;
    [SerializeField]Vector3 jumpPosition;
    [SerializeField]Vector3 sprintPosition;
    [SerializeField]Vector3 fallPosition;
    Vector3[] positions;

    [SerializeField]Vector3 crouchOffset = new Vector3(0f, -.75f, 0f);

    [SerializeField]float defaultFOV = 60;
    [SerializeField]float ironSightFOV = 30;
    [SerializeField]float jumpFOV = 80;
    [SerializeField]float sprintFOV = 70;
    [SerializeField]float fallFOV = 90;
    float[] fovs;

    [SerializeField]float maxCameraUpAngle;
    [SerializeField]float maxCameraDownAngle;

    float cameraRotationX;
    float cameraRotationY;

    [SerializeField]float targetDoFDistance;
    [SerializeField]float actualDoFDistance;
    [SerializeField]float focusSpeed = 5f;

    [SerializeField]float defaultDoFStrength = .25f;
    [SerializeField]float ironSightDoFStrength = 7f;

    Camera camera;
    GameObject target;

    CameraMode mode = CameraMode.Default;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        positions = new Vector3[] { defaultPosition, ironSightPosition, jumpPosition, sprintPosition, fallPosition };
        fovs = new float[] { defaultFOV, ironSightFOV, jumpFOV, sprintFOV, fallFOV };

        profile.TryGet(out dof);
        camera = this.GetComponentInChildren<Camera>();
        target = FindObjectOfType<PlayerActor>().gameObject;

        if (target == null)
            Debug.LogWarning("CameraController couldnt find Player object, did you forget to drag it into your scene?");

        GlobalEvents.Subscribe(GlobalEvent.SetCameraMode, SetMode);
    }
    void Update()
    {
        GatherInput();
        UpdateSettings();
    }

    void SetMode(object[] args) => mode = (CameraMode)args[0];

    void GatherInput()
    {
        cameraRotationX += Input.GetAxis("Mouse X") * sensitivityX * (Time.deltaTime / Time.timeScale);
        cameraRotationY += -Input.GetAxis("Mouse Y") * sensitivityY * (Time.deltaTime / Time.timeScale);
        cameraRotationY = Mathf.Clamp(cameraRotationY, -maxCameraUpAngle, maxCameraDownAngle);

        this.transform.rotation = Quaternion.Euler(cameraRotationY, cameraRotationX, 0f);

        if(target != null)
            this.transform.position = Vector3.Lerp(this.transform.position, target.transform.position, translationSpeed * (Time.deltaTime / Time.timeScale));
    }
    void UpdateSettings()
    {
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fovs[(int)mode], cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, positions[(int)mode], cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));


        Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, Mathf.Infinity);

        if (hit.transform != null)
            targetDoFDistance = hit.distance;

        actualDoFDistance = Mathf.Lerp(actualDoFDistance, targetDoFDistance, focusSpeed * (Time.deltaTime / Time.timeScale));
        dof.farFocusStart.value = actualDoFDistance + 2f;
        dof.farMaxBlur = mode == CameraMode.IronSight ? ironSightDoFStrength : defaultDoFStrength;

        dof.nearFocusStart.value = mode == CameraMode.IronSight ? 1f : 0f;
        dof.nearFocusEnd.value = mode == CameraMode.IronSight ? 1.5f : 0f;
    }
}

public enum CameraMode
{
    Default,
    IronSight,
    Jump,
    Sprint,
    Fall,
}