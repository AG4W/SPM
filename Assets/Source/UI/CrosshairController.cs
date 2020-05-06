using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    GameObject hitMarker;
    Image markerImage;

    [SerializeField]float hitFadeTime = .25f;
    [SerializeField]InterpolationMode mode = InterpolationMode.EaseOut;

    [SerializeField]float maxCrosshairSize = 1.25f;

    float fadeTimer = 0f;

    void Awake()
    {
        hitMarker = this.transform.FindRecursively("hit marker").gameObject;
        markerImage = hitMarker.transform.GetComponent<Image>();
        hitMarker.SetActive(false);

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
    }
    void OnHit(object[] args)
    {
        if (hitMarker != null)
        {
            fadeTimer = 0f;
            markerImage.color = new Color(1f, 1f, 1f, 1f);
            hitMarker.SetActive(true);
        }
    }
}
