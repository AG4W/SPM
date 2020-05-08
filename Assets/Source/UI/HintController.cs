using UnityEngine;
using UnityEngine.UI;

public class HintController : MonoBehaviour
{
    GameObject hint;

    Text text;
    Image sprite;

    PlayerActor player;
    GameObject target;

    Camera camera;

    void Awake()
    {
        hint = this.transform.FindRecursively("Hint").transform.GetChild(0).gameObject;
        text = hint.GetComponentInChildren<Text>();
        sprite = hint.GetComponentInChildren<Image>();
        camera = Camera.main;

        hint.SetActive(false);
        player = FindObjectOfType<PlayerActor>();
    }
    void Update()
    {
        if (hint.activeSelf)
        {
            if (Vector3.Dot(player.transform.forward, player.transform.position.DirectionTo(target.transform.position).normalized) >= .5f)
                hint.transform.position = camera.WorldToScreenPoint(target.transform.position);
            else
                hint.transform.position = camera.ViewportToScreenPoint(new Vector3(Vector3.Dot(Vector3.Cross(player.transform.forward, player.transform.position.DirectionTo(target.transform.position)), Vector3.up) > 0f ? 1f : 0f, .5f));
        }
    }

    public void DisplayHint(string t)
    {
        text.text = t;
        hint.SetActive(true);
    }
    public void SetHintPosition(GameObject go)
    {
        target = go;
    }
    public void CloseHint()
    {
        hint.SetActive(false);
    }
}
