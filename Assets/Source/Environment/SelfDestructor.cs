using UnityEngine;

public class SelfDestructor : MonoBehaviour
{
    [SerializeField]float lifetime = 2f;

    void Start()
    {
        if(lifetime > 0f)
            Destroy(this.gameObject, lifetime);
    }

    public void Invoke()
    {
        Destroy(this.gameObject);
    }
}
