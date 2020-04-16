using UnityEngine;

public class SelfDestructor : MonoBehaviour
{
    [SerializeField]float lifetime = 2f;

    void Start()
    {
        Destroy(this.gameObject, lifetime);
    }
}
