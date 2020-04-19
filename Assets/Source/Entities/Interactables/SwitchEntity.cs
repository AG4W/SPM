using UnityEngine;
using UnityEngine.UI;

public class SwitchEntity : ConnectedEntity
{
    [Header("Switch Settings")]
    [SerializeField]string onText;
    [SerializeField]string offText;

    [Header("UI Settings")]
    [SerializeField]GameObject worldUIRoot;

    [SerializeField]Color onColor = Color.blue;
    [SerializeField]Color offColor = Color.red;

    [SerializeField]Text[] texts;
    [SerializeField]Image[] images;
    [SerializeField]Light[] lights;

    [SerializeField]

    protected bool InOffState { get; set; } = false;
    protected override void Initalize()
    {
        base.Initalize();

        Material tm = Instantiate(texts.First().material);
        Material im = Instantiate(images.First().material);

        for (int i = 0; i < texts.Length; i++)
            texts[i].material = tm;
        for (int i = 0; i < images.Length; i++)
            images[i].material = im;

        UpdateWorldUI();
    }
    public override void OnInteractStart()
    {
        base.OnInteractStart();

        Switch();
        UpdateWorldUI();
    }

    protected override void OnLinkedStart(ConnectedEntity other)
    {
        base.OnLinkedStart(other);

        Switch();
        UpdateWorldUI();
    }
    protected override void OnLinkedComplete(ConnectedEntity other)
    {
        base.OnLinkedComplete(other);

        UpdateWorldUI();
    }

    public void Switch()
    {
        this.InOffState = !this.InOffState;
    }

    public virtual void OverrideWorldUI(string text, Color color)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].material.SetColor("_Color", color);
            texts[i].material.SetColor("_Emission", color * 16f);
            texts[i].color = color;
            texts[i].text = text;
        }
        for (int i = 0; i < images.Length; i++)
        {
            images[i].material.SetColor("_Color", color);
            images[i].material.SetColor("_Emission", color * 16f);
            images[i].color = color;
        }
        for (int i = 0; i < lights.Length; i++)
            lights[i].color = color;

        worldUIRoot.SetActive(true);
    }
    public void UpdateWorldUI()
    {
        OverrideWorldUI(this.InOffState ? offText : onText, this.InOffState ? offColor : onColor);
    }
    public virtual void CloseWorldUI()
    {
        worldUIRoot.SetActive(false);
    }
}
