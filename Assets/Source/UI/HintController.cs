using UnityEngine;
using UnityEngine.UI;

public class HintController : MonoBehaviour
{
    GameObject hint;

    Text text;
    Image sprite;

    GameObject target;

    Camera camera;

    void Awake()
    {
        hint = this.transform.FindRecursively("Hint").transform.GetChild(0).gameObject;
        text = hint.GetComponentInChildren<Text>();
        sprite = hint.GetComponentInChildren<Image>();
        camera = Camera.main;

        hint.SetActive(false);
    }
    void Update()
    {
        if (hint.activeSelf)
            hint.transform.position = camera.WorldToScreenPoint(target.transform.position);
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
