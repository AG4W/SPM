using UnityEngine;

using System.Collections;
using System;
//using System.Runtime.Remoting.Messaging;

public class SimpleDoor : MonoBehaviour, IPersistable
{
    [SerializeField]string prompt;
    [SerializeField]float interactionDistance = 4f;

    [Header("Door Configuration")]
    [SerializeField]GameObject door;

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

    float animationTime = 1f;
    Animator animator;

    [Header("Persistence")]
    [SerializeField]bool isPersistent = false;
                                           //aaaay lmao, time to get hit by murphy's law booooooois
    string IPersistable.Hash => this.name.ToString() + this.transform.position.ToString();
    bool IPersistable.IsPersistable => isPersistent;
    bool IPersistable.PersistBetweenScenes => false;
    
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
            panels[i].Initialize();
            panels[i].SetState(states[(int)state]);
            //if(this.state != SimpleDoorState.Broken && this.state != SimpleDoorState.Locked)
            panels[i].OnInteract += OnInteract;
        }

        animator = this.GetComponent<Animator>();
        animator.SetBool("isOpen", this.state == SimpleDoorState.Opened);

        //designers är retarded
        Debug.Assert(door != null, this.name + " does not have a door object assigned, did you forget to assign it?", this.gameObject);
        Debug.Assert(panels.Length > 0, this.name + " has no panels assigned and will not be interactable, remove this component if this is desired behaviour.", this.gameObject);
        Debug.Assert(state != SimpleDoorState.Animating, this.name + " should not start in animating mode, please select a different mode.", this.gameObject);
    }
    public void SetState(string state)
    {
        Enum.TryParse(state, true, out this.state);

        for (int i = 0; i < panels.Length; i++)
            panels[i].SetState(states[(int)this.state]);

        animator.SetBool("isOpen", this.state == SimpleDoorState.Opened);
    }

    void OnInteract(bool isRecursiveCall = false)
    {
        //stoppa icke-rekursiva anrop ifrån att trigga dörren.
        if (this.state == SimpleDoorState.Animating && !isRecursiveCall)
            return;

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
            case SimpleDoorState.Broken:
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
            animator.SetBool("isOpen", last == SimpleDoorState.Closed);

            float t = 0f;

            while (t < animationTime)
            {
                t += Time.deltaTime;
                yield return null;
            }

            OnInteract(true);
        }
    }

    void IPersistable.OnEnter(Context context)
    {
        this.SetState(context.data["state"].ToString());
        animator.SetBool("isOpen", this.state == SimpleDoorState.Opened);
    }
    Context IPersistable.GetContext()
    {
        Context c = new Context();
        c.data.Add("state", this.state);

        return c;
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