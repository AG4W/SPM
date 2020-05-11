using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Dialogue")]
public class Dialogue : ScriptableObject
{
    [Header("Dialogue displayed")]
    [Tooltip("You can use Rich Text markup. E.g: <color=gray><i>Hey!</i></color>")]
    [TextArea(3, 20)][SerializeField]string text;

    [Header("Only used if there is no audio clip")]
    [Tooltip("Only used if there is no audio clip, otherwise the length of the clip is used.")]
    [SerializeField] float time = 6f;

    [Header("Audio")]
    [SerializeField]AudioClip clip;


    [Header("Use this to chain dialogueclips together")]
    [Tooltip("Use this to chain dialogueclips together")]
    [SerializeField]Dialogue next;

    [SerializeField]UnityEvent events;

    public string Text => text;
    public float Time => clip == null ? time : clip.length;

    public AudioClip Clip => clip;

    public Dialogue Next => next;

    public UnityEvent Events => events;
}
