using UnityEngine;

public class Initializer : MonoBehaviour
{
    void Awake()
    {
        GlobalEvents.Initialize();

        Player.Initialize(null);

        GeneralAudioController.Initialize();
        DamageableController.Initialize();
    }
}
