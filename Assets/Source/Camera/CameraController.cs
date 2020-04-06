using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]float sensitivity = 2f;
    [SerializeField]float translationSpeed = 5f;

    [SerializeField]GameObject target;

    void Update()
    {
        GatherInput();
    }

    void GatherInput()
    {
        this.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime);
        this.transform.position = Vector3.Lerp(this.transform.position, target.transform.position, translationSpeed * Time.deltaTime);
    }
}
