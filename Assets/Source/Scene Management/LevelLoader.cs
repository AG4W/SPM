using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void LoadLevel(string scenename)
    {
        GlobalEvents.Raise(GlobalEvent.OnSceneExit, SceneManager.GetActiveScene());
        SceneManager.LoadScene(scenename);
    }
}
