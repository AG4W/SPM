using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class KillZone : MonoBehaviour
{
    PlayerActor player;
    BoxCollider box;

    void Awake()
    {
        player = FindObjectOfType<PlayerActor>();
        box = this.GetComponent<BoxCollider>();

        box.isTrigger = true;
    }
    void Update()
    {
        if (box.bounds.Contains(player.transform.position))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
