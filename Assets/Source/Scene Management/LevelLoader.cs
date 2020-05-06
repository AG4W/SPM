using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelLoader : MonoBehaviour
{

    public void LoadLevel(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }

}
