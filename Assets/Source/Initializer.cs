using UnityEngine;

public class Initializer : MonoBehaviour
{
    void Awake()
    {
        BulletAudioController.Initialize();
    }
}
