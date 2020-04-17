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
    [SerializeField]Vector3 crouchOffset = new Vector3(0f, -.75f, 0f);

    [SerializeField]float cameraWhiskerDistance = .5f;
    float cameraZBuffer = 0f;

    [SerializeField]float defaultFOV = 60;
    [SerializeField]float ironSightFOV = 30;

    [SerializeField]float maxCameraUpAngle;
    [SerializeField]float maxCameraDownAngle;

    float cameraRotationX;
    float cameraRotationY;

    [SerializeField]float lastFocusDistance;
    [SerializeField]float actualDoFDistance;
    [SerializeField]float focusSpeed = 5f;
    [SerializeField]float defaultDoFStrength = .25f;
    [SerializeField]float ironSightDoFStrength = 7f;

    Camera camera;
    GameObject target;

    AimMode mode = AimMode.Default;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        profile.TryGet(out dof);
        camera = this.GetComponentInChildren<Camera>();
        target = FindObjectOfType<LocomotionController>().gameObject;

        if (target == null)
            Debug.LogWarning("CameraController couldnt find Player object, did you forget to drag it into your scene?");

        //fixa surrogat för denna
        //GlobalEvents.Subscribe(GlobalEvent.SetActorTargetAimMode, SetMode);
    }
    void Update()
    {
        GatherInput();
        UpdateSettings();
    }

    void SetMode(object[] args)
    {
        mode = (AimMode)args[0];
    }

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
        RaycastBack();

        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, mode == AimMode.IronSight ? ironSightFOV : defaultFOV, cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));

        Vector3 finalCameraPos = mode == AimMode.IronSight ? ironSightPosition : defaultPosition;

        if (Input.GetKey(KeyCode.C))
            finalCameraPos += crouchOffset;

        //clampa så att kameran inte kan gå igenom spelaren
        //finalCameraPos.z += cameraZBuffer;

        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, finalCameraPos, cameraTranslationSpeed * (Time.deltaTime / Time.timeScale));

        Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, Mathf.Infinity);

        if (hit.transform != null)
            lastFocusDistance = hit.distance;

        actualDoFDistance = Mathf.Lerp(actualDoFDistance, lastFocusDistance, focusSpeed * (Time.deltaTime / Time.timeScale));
        dof.farFocusStart.value = actualDoFDistance + 2f;
        dof.farMaxBlur = mode == AimMode.IronSight ? ironSightDoFStrength : defaultDoFStrength;

        dof.nearFocusStart.value = mode == AimMode.IronSight ? 1f : 0f;
        dof.nearFocusEnd.value = mode == AimMode.IronSight ? 1.5f : 0f;
    }

    void RaycastBack()
    {
        Physics.Raycast(camera.transform.position, -camera.transform.forward, out RaycastHit hit, cameraWhiskerDistance);
        cameraZBuffer = Mathf.Lerp(cameraZBuffer, hit.transform != null ? -hit.distance : 0f, translationSpeed * (Time.deltaTime / Time.timeScale));
    }
}