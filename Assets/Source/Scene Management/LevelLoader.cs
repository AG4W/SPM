using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{

    public void LoadLevel(int sceneNr)
    {
        //GlobalEvents.Raise(GlobalEvent.SetPlayerCurrentCheckpoint, null);
        FindObjectOfType<PlayerActor>().clearCheckPoint();
        GlobalEvents.Raise(GlobalEvent.OnSceneExit, SceneManager.GetActiveScene());
        SceneManager.LoadScene(sceneNr);
    }
}
