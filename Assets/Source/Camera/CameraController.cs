using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]float sensitivity = 2f;
    [SerializeField]float translationSpeed = 5f;

    [SerializeField]GameObject target;

    [SerializeField]Vector3 defaultPosition;
    [SerializeField]Vector3 ironSightPosition;

    [SerializeField]float defaultFOV = 60;
    [SerializeField]float ironSightFOV = 30;

    Camera camera;

    bool inIronSights;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        camera = this.GetComponentInChildren<Camera>();
    }
    void Update()
    {
        GatherInput();

        UpdateSettings();
    }

    void GatherInput()
    {
        this.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime);
        this.transform.position = Vector3.Lerp(this.transform.position, target.transform.position, translationSpeed * Time.deltaTime);

        inIronSights = Input.GetKey(KeyCode.Mouse1);
    }
    void UpdateSettings()
    {
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, inIronSights ? ironSightFOV : defaultFOV, translationSpeed * Time.deltaTime);
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, inIronSights ? ironSightPosition : defaultPosition, translationSpeed * Time.deltaTime);
    }
}
