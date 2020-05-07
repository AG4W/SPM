using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    GameObject hitMarker;
    Image markerImage;
    Image crosshair;

    [SerializeField]Sprite[] sprites;

    [SerializeField]float hitFadeTime = .25f;
    [SerializeField]InterpolationMode mode = InterpolationMode.EaseOut;

    [SerializeField]float maxCrosshairSize = 1.25f;

    float fadeTimer = 0f;

    Vector2 targetScale;

    void Start()
    {
        hitMarker = this.transform.FindRecursively("hit marker").gameObject;
        markerImage = hitMarker.transform.GetComponent<Image>();
        hitMarker.SetActive(false);

        crosshair = this.transform.FindRecursively("dot").gameObject.GetComponent<Image>();
        crosshair.sprite = sprites[(int)CrosshairIcon.Default];
        targetScale = Vector2.one * 5f;

        GlobalEvents.Subscribe(GlobalEvent.SetCrosshairIcon, SetIcon);
        GlobalEvents.Subscribe(GlobalEvent.PlayerShotHit, OnHit);
    }
    void Update()
    {
        if (hitMarker.activeSelf)
        {
            fadeTimer += Time.deltaTime;
            markerImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, (fadeTimer / hitFadeTime).Interpolate(mode)));

            if(fadeTimer >= hitFadeTime)
            {
                fadeTimer = 0f;
                hitMarker.SetActive(false);
            }
        }

        crosshair.rectTransform.sizeDelta = Vector3.Lerp(crosshair.rectTransform.sizeDelta, targetScale, (20f * (Time.deltaTime / Time.timeScale)).Interpolate(InterpolationMode.Smoothstep));
    }

    void SetIcon(object[] args)
    {
        crosshair.sprite = sprites[(int)(CrosshairIcon)args[0]];
        targetScale = Vector2.one * ((CrosshairIcon)args[0] == CrosshairIcon.Default ? 5f : 50f);
    }
    void OnHit(object[] args)
    {
        fadeTimer = 0f;
        markerImage.color = new Color(1f, 1f, 1f, 1f);
        hitMarker.SetActive(true);
    }
}
public enum CrosshairIcon
{
    Default,
    Interact,
}
