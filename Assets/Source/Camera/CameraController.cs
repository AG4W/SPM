using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CameraController : MonoBehaviour
{
    [SerializeField]Volume volume;
    [SerializeField]VolumeProfile post;

    DepthOfField dof;

    [SerializeField]float sensitivityX = 2f;
    [SerializeField]float sensitivityY = 2f;
    [SerializeField]float translationSpeed = 5f;

    [SerializeField]GameObject target;

    [SerializeField]Vector3 defaultPosition;
    [SerializeField]Vector3 ironSightPosition;

    [SerializeField]float defaultFOV = 60;
    [SerializeField]float ironSightFOV = 30;

    [SerializeField]float maxCameraUpAngle;
    [SerializeField]float maxCameraDownAngle;

    float cameraRotationX;
    float cameraRotationY;

    Camera camera;

    bool inIronSights;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        camera = this.GetComponentInChildren<Camera>();
        //dof = volume.TryGetComponent(out dof);
    }
    void Update()
    {
        GatherInput();

        UpdateSettings();
    }

    void GatherInput()
    {
        cameraRotationX += Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        cameraRotationY += -Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;
        cameraRotationY = Mathf.Clamp(cameraRotationY, -maxCameraUpAngle, maxCameraDownAngle);

        this.transform.rotation = Quaternion.Euler(cameraRotationY, cameraRotationX, 0f);
        this.transform.position = Vector3.Lerp(this.transform.position, target.transform.position, translationSpeed * Time.deltaTime);

        inIronSights = Input.GetKey(KeyCode.Mouse1);
    }
    void UpdateSettings()
    {
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, inIronSights ? ironSightFOV : defaultFOV, translationSpeed * Time.deltaTime);
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, inIronSights ? ironSightPosition : defaultPosition, translationSpeed * Time.deltaTime);
    }
}
