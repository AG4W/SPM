using UnityEngine;
using System.Collections;

public class SimpleDoor : MonoBehaviour
{
    [SerializeField]string prompt;
    [SerializeField]float interactionDistance = 4f;

    [Header("Door Configuration")]
    [SerializeField]GameObject door;
    [SerializeField]Transform closedPosition;
    [SerializeField]Transform openPosition;

    [SerializeField]SimpleDoorState mode = SimpleDoorState.Closed;
    SimpleDoorState last;

    [Header("Panel Configuration")]
    [SerializeField]Panel[] panels;

    [SerializeField]PanelState closedState;
    [SerializeField]PanelState openedState;
    [SerializeField]PanelState animatingState;
    [SerializeField]PanelState lockedState;
    [SerializeField]PanelState brokenState;

    PanelState[] states;

    [Header("Animation Settings")]
    [SerializeField]float animationTime = 2f;
    [SerializeField]InterpolationMode animationMode = InterpolationMode.EaseIn;

    void Awake()
    {
        states = new PanelState[]
        {
            closedState,
            openedState,
            animatingState,
            lockedState,
            brokenState
        };

        //designers är retarded
        Debug.Assert(door != null, this.name + " does not have a door object assigned, did you forget to assign it?", this.gameObject);
        Debug.Assert(closedPosition != null, this.name + " does not have a closedPosition assigned, did you forget to assign it?", this.gameObject);
        Debug.Assert(openPosition != null, this.name + " does not have a openPosition object assigned, did you forget to assign it?", this.gameObject);
        Debug.Assert(panels.Length > 0, this.name + " has no panels assigned and will not be interactable, remove this component if this is desired behaviour.", this.gameObject);

        for (int i = 0; i < panels.Length; i++)
            panels[i].OnInteract += OnInteract;

        this.StartCoroutine(TransitionAsync(mode));
    }

    void OnInteract()
    {
        if (mode == SimpleDoorState.Broken || mode == SimpleDoorState.Locked)
            return;

        SimpleDoorState state;

        switch (mode)
        {
            case SimpleDoorState.Closed:
                state = SimpleDoorState.Animating;
                break;
            case SimpleDoorState.Opened:
                state = SimpleDoorState.Animating;
                break;
            case SimpleDoorState.Animating:
                state = last == SimpleDoorState.Closed ? SimpleDoorState.Opened : SimpleDoorState.Closed;
                break;
            case SimpleDoorState.Locked:
                return;
            default:
                state = SimpleDoorState.Opened;
                break;
        }

        last = mode;
        mode = state;

        this.StopAllCoroutines();
        this.StartCoroutine(TransitionAsync(state));
    }
    IEnumerator TransitionAsync(SimpleDoorState state)
    {
        for (int i = 0; i < panels.Length; i++)
            panels[i].SetState(states[(int)state]);

        if(state == SimpleDoorState.Animating)
        {
            Vector3 start = last == SimpleDoorState.Closed ? closedPosition.position : openPosition.position;
            Vector3 end = last == SimpleDoorState.Closed ? openPosition.position : closedPosition.position;

            float t = 0f;

            while (t <= animationTime)
            {
                t += Time.deltaTime;
                door.transform.localPosition = Vector3.Lerp(start, end, (t / animationTime).Interpolate(animationMode));
                yield return null;
            }

            OnInteract();
        }
    }
}
public enum SimpleDoorState
{
    Closed,
    Opened,
    Animating,
    Locked,
    Broken,
}