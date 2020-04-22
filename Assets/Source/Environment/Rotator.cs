using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]float speed = 5f;
    [SerializeField]Vector3 rotation = new Vector3(0f, 1f, 1f);

    void Update()
    {
        this.transform.eulerAngles += rotation.normalized * speed * Time.deltaTime;
    }
}
