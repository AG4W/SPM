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

    [SerializeField]SimpleDoorState state = SimpleDoorState.Closed;
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

        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetState(states[(int)state]);

            if(this.state != SimpleDoorState.Broken && this.state != SimpleDoorState.Locked)
                panels[i].OnInteract += OnInteract;
        }

        if (this.state == SimpleDoorState.Broken || this.state == SimpleDoorState.Locked)
        {
            Destroy(this);
            return;
        }

        //designers är retarded
        Debug.Assert(door != null, this.name + " does not have a door object assigned, did you forget to assign it?", this.gameObject);
        Debug.Assert(closedPosition != null, this.name + " does not have a closedPosition assigned, did you forget to assign it?", this.gameObject);
        Debug.Assert(openPosition != null, this.name + " does not have a openPosition object assigned, did you forget to assign it?", this.gameObject);
        Debug.Assert(panels.Length > 0, this.name + " has no panels assigned and will not be interactable, remove this component if this is desired behaviour.", this.gameObject);
        Debug.Assert(state != SimpleDoorState.Animating, this.name + " should not start in animating mode, please select a different mode.", this.gameObject);

        door.transform.localPosition = state == SimpleDoorState.Opened ? openPosition.localPosition : closedPosition.localPosition;
    }

    void OnInteract()
    {
        SimpleDoorState state;

        switch (this.state)
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

        last = this.state;
        this.state = state;

        this.StopAllCoroutines();
        this.StartCoroutine(TransitionAsync(state));
    }
    IEnumerator TransitionAsync(SimpleDoorState state)
    {
        for (int i = 0; i < panels.Length; i++)
            panels[i].SetState(states[(int)state]);

        if(state == SimpleDoorState.Animating)
        {
            Vector3 start = last == SimpleDoorState.Closed ? closedPosition.localPosition : openPosition.localPosition;
            Vector3 end = last == SimpleDoorState.Closed ? openPosition.localPosition : closedPosition.localPosition;

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