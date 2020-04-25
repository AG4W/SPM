using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
    [SerializeField]Image bar;

    PlayerActor player;

    void Awake()
    {
        player = FindObjectOfType<PlayerActor>();
    }

    void Update()
    {
        bar.fillAmount = player.Health.CurrentInPercent;
    }
}
