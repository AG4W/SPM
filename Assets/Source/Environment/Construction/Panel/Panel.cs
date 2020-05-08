using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System.Collections;

[RequireComponent(typeof(Collider))]
public class Panel : MonoBehaviour, IInteractable
{
    [SerializeField]string prompt = "";
    [SerializeField]float interactionDistance = 4f;

    [SerializeField]Text[] texts;
    [SerializeField]Image[] images;
    [SerializeField]Light[] lights;

    PanelState current;
    AudioSource source;

    [Header("Panel Configuration (ignored if connected to door)")]
    [SerializeField]PanelState state;

    [Header("Event Triggers")]
    [SerializeField]UnityEvent events;

    string IInteractable.Prompt => prompt;
    float IInteractable.InteractionDistance => interactionDistance;

    Vector3 IInteractable.Position => this.transform.position;

    void Awake()
    {
        texts = texts ?? new Text[0];
        images = images ?? new Image[0];
        lights = lights ?? new Light[0];

        if(texts.Length > 0)
        {
            Material tm = texts[0].material;

            for (int i = 0; i < texts.Length; i++)
                texts[i].material = Instantiate(tm);
        }
        if(images.Length > 0)
        {
            Material im = images[0].material;

            for (int i = 0; i < images.Length; i++)
                images[i].material = Instantiate(im);
        }

        source = this.GetComponentInChildren<AudioSource>();
        this.gameObject.layer = LayerMask.NameToLayer("Interactable");

        this.SetState(state);
    }

    public void Interact()
    {
        if(current.InteractSFX.Length > 0)
            source.PlayOneShot(current.InteractSFX.Random());

        events?.Invoke();
        OnInteract?.Invoke();
    }

    public void SetState(PanelState state)
    {
        current = state;
        UpdatePanel();
    }
    void UpdatePanel()
    {
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = current.Text;
            texts[i].material.SetColor("_Color", current.Tint);
            texts[i].material.SetColor("_Emission", current.Tint * current.TintIntensity);
        }
        for (int i = 0; i < images.Length; i++)
        {
            images[i].sprite = current.Sprite;
            images[i].material.SetColor("_Color", current.Tint);
            images[i].material.SetColor("_Emission", current.Tint * current.TintIntensity);
        }
        for (int i = 0; i < lights.Length; i++)
            lights[i].color = current.Tint;

        if (current.HasBlinkEffect)
            StartCoroutine(BlinkAsync());
    }

    IEnumerator BlinkAsync()
    {
        while (current.HasBlinkEffect)
        {
            OnBlink(false);
            yield return new WaitForSeconds(current.BlinkLength);

            OnBlink(true);

            if (current.BlinkSFX.Length > 0)
                source.PlayOneShot(current.BlinkSFX.Random());

            yield return new WaitForSeconds(current.TimeBetweenBlinks);
        }

        OnBlink(true);
    }
    void OnBlink(bool status)
    {
        if (current.BlinkText)
        {
            for (int i = 0; i < texts.Length; i++)
                texts[i].gameObject.SetActive(status);
        }

        if (current.BlinkImages)
        {
            for (int i = 0; i < images.Length; i++)
                images[i].gameObject.SetActive(status);
        }

        for (int i = 0; i < lights.Length; i++)
            lights[i].gameObject.SetActive(status);
    }

    public delegate void OnInteractEvent();
    public OnInteractEvent OnInteract;
}
