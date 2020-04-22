using UnityEngine;

using System;

//serialiserbar klass som bara håller massa data för paneler
//används för att kunna customisera detta för olika typer av dörrar etc
[Serializable]
public class PanelState
{
    [Header("Common Settings")]
    [SerializeField]string text = "REPLACE ME";

    [SerializeField]Sprite sprite;
    [SerializeField]Color tint = Color.magenta;
    [SerializeField]float tintIntensity = 8f;

    [Header("Blink Settings")]
    [SerializeField]bool blinkText = false;
    [SerializeField]bool blinkImages = false;

    [SerializeField]float blinkLength = .25f;
    [SerializeField]float timeBetweenBlinks = .25f;

    public string Text => text;

    public Sprite Sprite => sprite;
    public Color Tint => tint;
    public float TintIntensity => tintIntensity;

    public bool HasBlinkEffect => this.BlinkText || this.BlinkImages;
    public bool BlinkText => blinkText;
    public bool BlinkImages => blinkImages;
    public float BlinkLength => blinkLength;
    public float TimeBetweenBlinks => timeBetweenBlinks;
}