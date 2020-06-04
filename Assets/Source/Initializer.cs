using UnityEngine;
using UnityEngine.SceneManagement;

public class Initializer : MonoBehaviour
{
    void Awake()
    {
        GlobalEvents.Initialize();
    }
    void Start()
    {
        WeaponDatabase.Initialize();
        PersistentData.Initialize();

        GeneralAudioController.Initialize();
        DamageableController.Initialize();

        GlobalEvents.Raise(GlobalEvent.OnSceneEnter, SceneManager.GetActiveScene());
    }
}
