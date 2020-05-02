using UnityEngine;

public class Initializer : MonoBehaviour
{
    void Awake()
    {
        Player.Initialize(null);

        BulletAudioController.Initialize();
    }
}
