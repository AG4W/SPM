using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Dialogue")]
public class Dialogue : ScriptableObject
{
    [TextArea(3, 20)][SerializeField]string text;
    [Header("Används bara ifall det inte finns något ljudklipp, annars används klippets längd.")]
    [SerializeField]float time = 6f;

    [SerializeField]AudioClip clip;

    [Header("Use this to chain dialogueclips together")]
    [SerializeField]Dialogue next;

    [SerializeField]UnityEvent events;

    public string Text => text;
    public float Time => clip == null ? time : clip.length;

    public AudioClip Clip => clip;

    public Dialogue Next => next;

    public UnityEvent Events => events;
}
