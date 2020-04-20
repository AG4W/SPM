using UnityEngine;

public class CoroutineSurrogate : MonoBehaviour
{
    public static CoroutineSurrogate instance { get; private set; }

    void Awake()
    {
        instance = this;
    }
}
